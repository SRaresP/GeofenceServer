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

		public ArrayList GeoFences { get; set; } = new ArrayList(0);

        public enum GeoAreaMode
		{
			NONE,
			ALERT_WHEN_INSIDE,
            ALERT_WHEN_OUTSIDE
		}
		public GeoArea() { }

		public static ArrayList GetGeoAreaList(long overseerId, long targetId)
		{
			GeoArea[] geoAreas;
			{
				GeoArea geoArea = new GeoArea()
				{
					OverseerId = overseerId,
					TargetId = targetId
				};
				geoAreas = geoArea.LoadMultipleUsingAvailableData().Cast<GeoArea>().ToArray();
			}

			ArrayList geoAreaList = new ArrayList(1);
			foreach (GeoArea geoArea in geoAreas)
			{
				geoAreaList.Add(geoArea);
				GeoFence geoFence = new GeoFence()
				{
					GeoAreaId = geoArea.Id
				};
				GeoFence[] geoFences = geoFence.LoadMultipleUsingAvailableData().Cast<GeoFence>().ToArray();
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
		public GeoArea(GeoArea toCopy) : base(toCopy) { }

		public GeoArea(string geoArea, long overseerId, long targetId, bool parseId = false)
		{
			if (!geoArea.Contains(AREA_FENCE_SEPARATOR)) throw new FormatException($"{GetType().Name} is not parsable. Passed ${GetType().Name} string: " + geoArea);
			string[] input = geoArea.Split(AREA_FENCE_SEPARATOR);
			string[] areaDetails = input[0].Split(AREA_DETAILS_SEPARATOR);
			if (areaDetails.Length < 6) throw new FormatException($"{GetType().Name} is not parsable. Passed {GetType().Name} string: " + geoArea);
			string[] geoFences = new string[0];
			if (input.Length > 1)
			{
				geoFences = input[1].Split(new char[] { FENCES_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
			}
			if (parseId)
			{
				Id = int.Parse(areaDetails[0]);
			}
			else
			{
				Id = DEFAULT_ID;
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
				throw new FormatException($"{GetType().Name} is not parsable. Passed {GetType().Name} string: " + geoArea);
			}
			TriggerMessage = areaDetails[5];
			GeoFences = new ArrayList(geoFences.Length);
			foreach (string geofenceStr in geoFences)
			{
				GeoFence geoFence = new GeoFence(geofenceStr, Id);
				GeoFences.Add(geoFence);
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
