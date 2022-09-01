using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeofenceServer.Data
{
    //This class is used to process locations,
    //which in the context of this entire server,
    //are stored exclusively as strings for ease of storage.
    //Think of locationHistory as an array of strings which is accessed
    //by using this utility class.
    class LocationHandler
    {
        private const int CAPACITY = 4;
        //LOC_SEPARATOR is currently 253 in ASCII
        //Used to separate the date, latitude and longitude within a location string
        private const char DATE_LAT_LONG_SEPARATOR = '²';
        private const char LOC_HISTORY_SEPARATOR = 'ⁿ';
        private LocationHandler() { }
        public static string AddLocation(string locationHistory, string locationToAdd)
        {
			if (locationHistory is null)
			{
				throw new ArgumentNullException("locationHistory was null");
			}

			if (locationToAdd is null)
			{
				throw new ArgumentNullException("locationToAdd was null");
			}
			//get the amount of locations in this location history string
			//and the index of the oldest known location
			int locationsCount = 0;
            int oldestLocationStartIndex = locationHistory.Length;

            for(int i = 0; i < locationHistory.Length; ++i)
			{
                if (locationHistory[i] == LOC_HISTORY_SEPARATOR)
				{
                    ++locationsCount;
                    if (locationsCount == CAPACITY - 1)
					{
                        oldestLocationStartIndex = i + 1;
					}
				}
			}

            //insert according to whether the history is full or not
            locationToAdd += LOC_HISTORY_SEPARATOR;
            if (locationsCount >= CAPACITY) //full case
            {
                locationHistory = locationHistory.Remove(oldestLocationStartIndex, locationHistory.Length - oldestLocationStartIndex);
            }
            return locationToAdd + locationHistory;
        }
        public static string getLocationByIndex(string locationHistory, int locationIndex)
		{
            return locationHistory.Split(LOC_HISTORY_SEPARATOR)[locationIndex];
        }
        public static string getLastLocation(string locationHistory)
        {
            return locationHistory.Split(LOC_HISTORY_SEPARATOR)[0];
        }
    }
}
