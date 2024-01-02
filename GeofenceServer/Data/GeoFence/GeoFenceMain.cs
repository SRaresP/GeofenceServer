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
		private static int DEFAULT_COORD = 999;

		public GeoFence(long geoAreaId, double latitude, double longitude, int radiusMeters)
		{
			Id = -1;
			GeoAreaId = geoAreaId;
			Latitude = latitude;
			Longitude = longitude;
			RadiusMeters = radiusMeters;
		}

		public GeoFence()
		{
			Id = -1;
			GeoAreaId = -1;
			Latitude = DEFAULT_COORD;
			Longitude = DEFAULT_COORD;
			RadiusMeters = -1;
		}

		public GeoFence(string geofence, long geoAreaId)
		{
			Id = -1;
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
		public GeoFence(GeoFence toCopy) : base(toCopy) { }

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
