﻿using System;
using System.Linq;
using System.Reflection;
using GeofenceServer.Util;

namespace GeofenceServer.Data
{
    public partial class TargetUser : DatabaseClient
    {
        public const int DEFAULT_NR_CODE_GENS = -1;

        public TargetUser(string Email, string Name, string PasswordHash, int nrOfCodeGenerations = 0, string LocationHistory = "")
        {
            this.Id = -1;
            this.Email = Email;
            this.Name = Name;
            this.PasswordHash = PasswordHash;
            this.NrOfCodeGenerations = nrOfCodeGenerations;
            this.LocationHistory = LocationHistory;
        }

        public TargetUser()
        {
            Id = -1;
            Email = "";
            Name = "";
            PasswordHash = "";
            LocationHistory = "";
            NrOfCodeGenerations = DEFAULT_NR_CODE_GENS;
        }

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
