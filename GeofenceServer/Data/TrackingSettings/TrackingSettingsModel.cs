using System;
using System.Collections.Generic;
using System.Linq;

namespace GeofenceServer.Data
{
    public partial class TrackingSettings : DatabaseClient
    {
        new public static string TableName => "tracking_settings";
        public long OverseerId { get; set; }
        public long TargetId { get; set; }
        public int Interval { get; set; }

        static TrackingSettings()
        {
            int result = ExecuteNonQuery(
                $"CREATE TABLE IF NOT EXISTS {TableName} " +
                $"(overseer_id BIGINT NOT NULL, " +
                $"target_id BIGINT NOT NULL, " +
                $"`interval` INTEGER, " +
                $"PRIMARY KEY (target_id, overseer_id), " +
                $"FOREIGN KEY(overseer_id) REFERENCES overseer_user(id), " +
                $"FOREIGN KEY(target_id) REFERENCES target_user(id) " +
                $");");
        }

        public override void Add()
        {
            throw new NotImplementedException("Use the Save() method.");
        }

        public override int Delete()
        {
            return ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE target_id='{TargetId}' AND overseer_id='{OverseerId}';");
        }

        public override void Update()
        {
            throw new NotImplementedException("Use the Save() method.");
        }

        public override void Save()
        {
            if (OverseerId == DEFAULT_ID || TargetId == DEFAULT_ID)
            {
                throw new DatabaseException($"{GetType().Name} composite key is missing a part.");
            }

            int nrRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (overseer_id, target_id, `interval`) " +
                $"VALUES ({OverseerId}, {TargetId}, {Interval}) " +
                $"ON DUPLICATE KEY UPDATE " +
                $"`interval` = {Interval};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to save {GetType().Name} (target_id = {TargetId}, overseer_id = {OverseerId}) to database.");
            }
        }

        protected override void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (OverseerId != DEFAULT_ID) conditions.Add($"overseer_id = {OverseerId}");
            else columnsToSelect.Add($"overseer_id");
            if (TargetId != DEFAULT_ID) conditions.Add($"target_id = {TargetId}");
            else columnsToSelect.Add($"target_id");
            // doesn't make much sense, but whatever
            if (Interval != DEFAULT_INTERVAL) conditions.Add($"`interval` = {Interval}");
            else columnsToSelect.Add($"`interval`");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException($"No data available to load {GetType().Name}s by.");
            }
        }

        public override bool IsLoaded()
        {
            return (TargetId != DEFAULT_ID) && (OverseerId != DEFAULT_ID);
        }
    }
}
