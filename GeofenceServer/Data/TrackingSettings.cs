using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeofenceServer.Data
{
    public class TrackingSettings
    {
        // Default interval in miliseconds
        // public const int  DEFAULT_INTERVAL = 1800000;
        public const int DEFAULT_INTERVAL = 3000;
        [Key, Column(Order = 0)]
        public int OverseerId { get; set; }
        [Key, Column(Order = 1)]
        public int TargetId { get; set; }
        [Required(ErrorMessage = "Interval missing while manipulating database", ErrorMessageResourceName = "Interval")]
        public int Interval { get; set; }

        public TrackingSettings(int overseerId, int targetId, int interval)
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

    public class TrackingSettingsDbContext : DbContext
    {
        public DbSet<TrackingSettings> TrackingSettings { get; set; }
    }
}
