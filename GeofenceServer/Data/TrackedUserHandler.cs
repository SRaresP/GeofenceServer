using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeofenceServer.Data
{
	public class TrackedUserHandler
    {
        //SHOULD BE THE SAME AS THE CAPACITY SET IN OVERSEERAPP
        private const int CAPACITY = 20;
        //LOC_SEPARATOR is currently 253 in ASCII
        //Used to separate the date, latitude and longitude within a location string
        private TrackedUserHandler() { }
        public static string AddTrackedUser(string trackedUsers, string trackedUserToAdd)
        {
            if (trackedUsers is null)
            {
                throw new ArgumentNullException("trackedUsers was null");
            }

            if (trackedUserToAdd is null)
            {
                throw new ArgumentNullException("trackedUserToAdd was null");
            }
            //get the amount of locations in this trackedUsers string
            int trackedUsersCount = 0;

            for (int i = 0; i < trackedUsers.Length; ++i)
            {
                if (trackedUsers[i] == Program.TRACKED_USERS_SEPARATOR)
                {
                    ++trackedUsersCount;
                }
            }

            //insert if not at max capacity
            trackedUserToAdd += Program.TRACKED_USERS_SEPARATOR;
            return trackedUserToAdd + trackedUsers;
        }
        public static string GetUserByIndex(string trackedUsers, int userIndex)
        {
            return trackedUsers.Split(Program.TRACKED_USERS_SEPARATOR)[userIndex];
        }
        public static string RemoveUser(string trackedUsers, string idToRemove)
		{
            int removalIndex = trackedUsers.IndexOf(idToRemove);
            trackedUsers.Remove(removalIndex, idToRemove.Length + 1);
            return trackedUsers;
		}
    }
}
