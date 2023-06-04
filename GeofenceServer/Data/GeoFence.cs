using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeofenceServer.Data
{
	public class GeoFence
	{
		private const char FENCE_DETAILS_SEPARATOR = '⌡';

		[Key, Column(Order = 0)]
		public int Id { get; set; }
		[Required(ErrorMessage = "GeoAreaId missing while manipulating database", ErrorMessageResourceName = "GeoFence")]
		public int GeoAreaId { get; set; }
		[Required(ErrorMessage = "Latitude missing while manipulating database", ErrorMessageResourceName = "GeoFence")]
		public double Latitude { get; set; }
		[Required(ErrorMessage = "Longitude missing while manipulating database", ErrorMessageResourceName = "GeoFence")]
		public double Longitude { get; set; }
		[Required(ErrorMessage = "Radius missing while manipulating database", ErrorMessageResourceName = "GeoFence")]
		public int RadiusMeters { get; set; }

		public GeoFence(int geoAreaId, double latitude, double longitude, int radiusMeters)
		{
			GeoAreaId = geoAreaId;
			Latitude = latitude;
			Longitude = longitude;
			RadiusMeters = radiusMeters;
		}

		// Shouldn't be used, values are set so that it's as obvious on a map as possible
		public GeoFence()
		{
			GeoAreaId = -1;
			Latitude = 0;
			Longitude = 0;
			// 10k kilometers
			RadiusMeters = 10000000;
		}

		public GeoFence(string geofence, int geoAreaId)
		{
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

	public class GeoFenceDbContext : DbContext
	{
		public DbSet<GeoFence> GeoFences { get; set; }
	}
}
