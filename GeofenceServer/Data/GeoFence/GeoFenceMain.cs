using System;
using System.Linq;
using System.Reflection;
using GeofenceServer.Util;

namespace GeofenceServer.Data
{
    public partial class GeoFence : DatabaseClient
    {
        private const char FENCE_DETAILS_SEPARATOR = '⌡';
		// Lat or lon can never get to 999.
		public static int DEFAULT_COORD = 999;

		public GeoFence()
		{
			Id = DEFAULT_ID;
			GeoAreaId = DEFAULT_ID;
			Latitude = DEFAULT_COORD;
			Longitude = DEFAULT_COORD;
			RadiusMeters = -1;
		}
		public GeoFence(GeoFence toCopy) : base(toCopy) { }

		public GeoFence(string geofence, long geoAreaId)
		{
			Id = DEFAULT_ID;
			string[] input = geofence.Split(FENCE_DETAILS_SEPARATOR);
			if (input.Length < 5)
			{
				throw new FormatException("GeoFence is not parsable. Passed GeoFence string: " + geofence);
			}
			Id = int.Parse(input[0]);
			GeoAreaId = geoAreaId;
			Latitude = double.Parse(input[2]);
			Longitude = double.Parse(input[3]);
			RadiusMeters = int.Parse(input[4]);
		}

		public override string ToString()
		{
			return Id.ToString() +
				FENCE_DETAILS_SEPARATOR +
				GeoAreaId +
				FENCE_DETAILS_SEPARATOR +
				Latitude +
				FENCE_DETAILS_SEPARATOR +
				Longitude +
				FENCE_DETAILS_SEPARATOR +
				RadiusMeters;
		}
	}
}
