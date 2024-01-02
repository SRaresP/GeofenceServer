using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeofenceServer.Data
{
    public partial class GeoArea : DatabaseClient
    {
		public static int DEFAULT_COLOR = 0xffffff;

        private const char AREA_FENCE_SEPARATOR = '·';
        private const char AREA_DETAILS_SEPARATOR = '°';
        private const char FENCES_SEPARATOR = '≈';
        private const char AREAS_SEPARATOR = '÷';

        public ArrayList GeoFences { get; set; }

        public enum GeoAreaMode
		{
			NONE,
			ALERT_WHEN_INSIDE,
            ALERT_WHEN_OUTSIDE
		}

		public static ArrayList GetGeoAreaList(long overseerId, long targetId)
		{
			GeoArea[] geoAreas;
			{
				GeoArea geoArea = new GeoArea()
				{
					OverseerId = overseerId,
					TargetId = targetId
				};
				try
				{
					geoAreas = geoArea.LoadMultipleUsingAvailableData();
				}
				catch (TableEntryDoesNotExistException)
				{
					geoAreas = new GeoArea[0];
				}
			}

			ArrayList geoAreaList = new ArrayList(1);
			foreach (GeoArea geoArea in geoAreas)
			{
				geoAreaList.Add(geoArea);
				GeoFence geoFence = new GeoFence()
				{
					GeoAreaId = geoArea.Id
				};
				GeoFence[] geoFences;
				try
				{
					geoFences = geoFence.LoadMultipleUsingAvailableData();
				}
				catch (TableEntryDoesNotExistException)
				{
					geoFences = new GeoFence[0];
				}
				foreach (GeoFence geoF in geoFences)
				{
					geoArea.GeoFences.Add(geoF);
				}
			}
			return geoAreaList;
		}

		public static string GetGeoAreaStr(long overseerId, long targetId)
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

		public GeoArea(long overseerId, long targetId, int color, GeoAreaMode mode, string triggerMessage)
		{
			Id = -1;
			OverseerId = overseerId;
			TargetId = targetId;
			Color = color;
			Mode = mode;
			TriggerMessage = triggerMessage;
			GeoFences = new ArrayList(1);
		}

		public GeoArea(long overseerId, long targetId, int color, GeoAreaMode mode)
		{
			Id = -1;
			OverseerId = overseerId;
			TargetId = targetId;
			Color = color;
			Mode = mode;
			TriggerMessage = "";
			GeoFences = new ArrayList(1);
		}

		public GeoArea(long overseerId, long targetId, int color)
		{
			Id = -1;
			OverseerId = overseerId;
			TargetId = targetId;
			Color = color;
			Mode = GeoAreaMode.NONE;
			TriggerMessage = "";
			GeoFences = new ArrayList(1);
		}

		public GeoArea(long overseerId, long targetId)
		{
			Id = -1;
			OverseerId = overseerId;
			TargetId = targetId;
			Color = DEFAULT_COLOR;
			Mode = GeoAreaMode.NONE;
			TriggerMessage = "";
			GeoFences = new ArrayList(1);
		}

		public GeoArea()
		{
			Id = -1;
			OverseerId = -1;
			TargetId = -1;
			Color = DEFAULT_COLOR;
			Mode = GeoAreaMode.NONE;
			TriggerMessage = "";
			GeoFences = new ArrayList(0);
		}
		public GeoArea(GeoArea toCopy) : base(toCopy) { }

		public GeoArea(string geoArea, long overseerId, long targetId, bool parseId = false)
		{
			if (!geoArea.Contains(AREA_FENCE_SEPARATOR)) throw new FormatException("GeoArea is not parsable. Passed GeoArea string: " + geoArea);
			string[] input = geoArea.Split(AREA_FENCE_SEPARATOR);
			string[] areaDetails = input[0].Split(AREA_DETAILS_SEPARATOR);
			if (areaDetails.Length < 6) throw new FormatException("GeoArea is not parsable. Passed GeoArea string: " + geoArea);
			string[] geoFences = new string[0];
			if (input.Length > 1)
			{
				geoFences = input[1].Split(new char[] { FENCES_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
			}
			// CHECK APP FOR PASSING GEOFENCES WITH ID = 0
			if (parseId)
			{
				Id = int.Parse(areaDetails[0]);
			}
			else
			{
				Id = -1;
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
				}
				catch (Exception e)
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
}
