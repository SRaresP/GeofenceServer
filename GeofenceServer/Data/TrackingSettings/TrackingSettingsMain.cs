using System;
using System.Reflection;

namespace GeofenceServer.Data
{
    public partial class TrackingSettings : DatabaseClient
    {
        // Default interval in miliseconds
        // public const int  DEFAULT_INTERVAL = 1800000;
        public const int DEFAULT_INTERVAL = 3000;
        public const int DEFAULT_ID = -1;

        public TrackingSettings(long overseerId, long targetId, int interval)
		{
            OverseerId = overseerId;
            TargetId = targetId;
            Interval = interval;
        }

        public TrackingSettings()
        {
            OverseerId = -1;
            TargetId = -1;
            Interval = DEFAULT_INTERVAL;
        }
    }
}
