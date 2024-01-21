using System;
using System.Reflection;

namespace GeofenceServer.Data
{
    public partial class TrackingSettings : DatabaseClient
    {
        // Default interval in miliseconds
        // public const int  DEFAULT_INTERVAL = 1800000;
        public const int DEFAULT_INTERVAL = 3000;

        public TrackingSettings()
        {
            OverseerId = DEFAULT_ID;
            TargetId = DEFAULT_ID;
            Interval = DEFAULT_INTERVAL;
        }
        public TrackingSettings(TrackingSettings toCopy) : base(toCopy) { }
    }
}
