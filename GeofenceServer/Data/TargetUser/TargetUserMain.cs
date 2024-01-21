using System;
using System.Linq;
using System.Reflection;
using GeofenceServer.Util;

namespace GeofenceServer.Data
{
    public partial class TargetUser : DatabaseClient
    {
        public const int DEFAULT_NR_CODE_GENS = -1;

        public TargetUser()
        {
            Id = DEFAULT_ID;
            Email = "";
            Name = "";
            PasswordHash = "";
            LocationHistory = "";
            NrOfCodeGenerations = DEFAULT_NR_CODE_GENS;
        }
        public TargetUser(TargetUser toCopy) : base(toCopy) { }

        public override string ToString()
        {
            return Email + Program.USER_SEPARATOR +
                Name + Program.USER_SEPARATOR +
                PasswordHash + Program.USER_SEPARATOR +
                LocationHandler.truncateHistoryForTransmission(LocationHistory) + Program.USER_SEPARATOR
                + Id;
        }
    }
}
