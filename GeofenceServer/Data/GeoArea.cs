using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeofenceServer.Data
{
	public class GeoArea
	{
		private const char AREA_FENCE_SEPARATOR = '·';
		private const char AREA_DETAILS_SEPARATOR = '°';
		private const char FENCES_SEPARATOR = '≈';
		private const char AREAS_SEPARATOR = '÷';

		public enum GeoAreaMode
		{
			ALERT_WHEN_INSIDE,
			ALERT_WHEN_OUTSIDE
		}

		[Key, Column(Order = 0)]
		public int Id { get; set; }
		[Key, Column(Order = 1)]
		public int OverseerId { get; set; }
		[Key, Column(Order = 2)]
		public int TargetId { get; set; }
		[Required(ErrorMessage = "Color missing while manipulating database", ErrorMessageResourceName = "GeoArea")]
		public int Color { get; set; }
		[Required(ErrorMessage = "Mode missing while manipulating database", ErrorMessageResourceName = "GeoArea")]
		public GeoAreaMode Mode { get; set; }
		[Required(ErrorMessage = "Mode missing while manipulating database", ErrorMessageResourceName = "GeoArea")]
		public string TriggerMessage { get; set; }
		[NotMapped]
		public ArrayList GeoFences { get; set; }

		public static ArrayList GetGeoAreaList(int overseerId, int targetId)
		{
			using (GeoAreaDbContext geoDbContext = new GeoAreaDbContext())
			{
				var geoAreas = from areas in geoDbContext.Areas
							   where areas.OverseerId == overseerId
							   where areas.TargetId == targetId
							   select areas;
				ArrayList geoAreaList = new ArrayList(1);
				foreach (GeoArea geoArea in geoAreas)
				{
					geoAreaList.Add(geoArea);
					using (GeoFenceDbContext fenceDbContext = new GeoFenceDbContext())
					{
						var geoFences = from fences in fenceDbContext.GeoFences
										where fences.GeoAreaId == geoArea.Id
										select fences;
						foreach (GeoFence geoFence in geoFences)
						{
							geoArea.GeoFences.Add(geoFence);
						}
					}
				}
				return geoAreaList;
			}
		}

		public static string GetGeoAreaStr(int overseerId, int targetId)
		{
			ArrayList geoAreas = GetGeoAreaList(overseerId, targetId);
			if (geoAreas.Count < 1) return "";
			string output = geoAreas[0].ToString();
			for (int index = 1; index < geoAreas.Count; ++index)
			{
				output += AREAS_SEPARATOR + geoAreas[index].ToString();
			}
			return output;
		}

		public GeoArea(int overseerId, int targetId, int color, GeoAreaMode mode, string triggerMessage)
		{
			OverseerId = overseerId;
			TargetId = targetId;
			Color = color;
			Mode = mode;
			TriggerMessage = triggerMessage;
			GeoFences = new ArrayList(1);
		}

		public GeoArea(int overseerId, int targetId, int color, GeoAreaMode mode)
		{
			OverseerId = overseerId;
			TargetId = targetId;
			Color = color;
			Mode = mode;
			TriggerMessage = "";
			GeoFences = new ArrayList(1);
		}

		public GeoArea(int overseerId, int targetId, int color)
		{
			OverseerId = overseerId;
			TargetId = targetId;
			Color = color;
			Mode = GeoAreaMode.ALERT_WHEN_INSIDE;
			TriggerMessage = "";
			GeoFences = new ArrayList(1);
		}

		public GeoArea(int overseerId, int targetId)
		{
			OverseerId = overseerId;
			TargetId = targetId;
			Color = 0xffffff;
			Mode = GeoAreaMode.ALERT_WHEN_INSIDE;
			TriggerMessage = "";
			GeoFences = new ArrayList(1);
		}

		public GeoArea()
		{
			OverseerId = -1;
			TargetId = -1;
			Color = 0xffffff;
			Mode = GeoAreaMode.ALERT_WHEN_INSIDE;
			TriggerMessage = "";
			GeoFences = new ArrayList(0);
		}

		public GeoArea(string geoArea, int overseerId, int targetId, bool parseId = false)
		{
			if (!geoArea.Contains(AREA_FENCE_SEPARATOR)) throw new FormatException("GeoArea is not parsable. Passed GeoArea string: " + geoArea);
			string[] input = geoArea.Split(AREA_FENCE_SEPARATOR);
			string[] areaDetails = input[0].Split(AREA_DETAILS_SEPARATOR);
			if (areaDetails.Length < 6) throw new FormatException("GeoArea is not parsable. Passed GeoArea string: " + geoArea);
			string[] geoFences = new string[0];
			if (input.Length > 1) {
				geoFences = input[1].Split(new char[] { FENCES_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
			}
			if (parseId)
			{
				Id = int.Parse(areaDetails[0]);
			}
			OverseerId = overseerId;
			TargetId = targetId;
			Color = int.Parse(areaDetails[3]);
			GeoAreaMode inputMode;
			if (Enum.TryParse<GeoAreaMode>(areaDetails[4], out inputMode))
			{
				Mode = inputMode;
			}
			else
			{
				throw new FormatException("GeoArea is not parsable. Passed GeoArea string: " + geoArea);
			}
			TriggerMessage = areaDetails[5];
			GeoFences = new ArrayList(geoFences.Length);
			foreach (string geofenceStr in geoFences)
			{
				try
				{
					GeoFence geoFence = new GeoFence(geofenceStr, Id);
					GeoFences.Add(geoFence);
				} catch (Exception e)
				{
					Trace.TraceError(e.Message);
					Trace.TraceError(e.StackTrace);
				}
			}
		}

		public override string ToString()
		{
			string output = Id.ToString() +
				AREA_DETAILS_SEPARATOR +
				OverseerId +
				AREA_DETAILS_SEPARATOR +
				TargetId +
				AREA_DETAILS_SEPARATOR +
				Color +
				AREA_DETAILS_SEPARATOR +
				Mode +
				AREA_DETAILS_SEPARATOR +
				TriggerMessage +
				AREA_FENCE_SEPARATOR;
			foreach (GeoFence geoFence in GeoFences)
			{
				output += geoFence.ToString() + FENCES_SEPARATOR;
			}
			return output;
		}
	}

	public class GeoAreaDbContext : DbContext
	{
		public DbSet<GeoArea> Areas { get; set; }
	}
}
