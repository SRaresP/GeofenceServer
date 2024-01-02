using System;
using System.Collections.Generic;
using System.Linq;

namespace GeofenceServer.Data
{
    public partial class TrackingSettings : DatabaseClient
    {
        public long OverseerId { get; set; }
        public long TargetId { get; set; }
        public int Interval { get; set; }

        static TrackingSettings()
        {
            int result = ExecuteNonQuery(
                $"CREATE TABLE IF NOT EXISTS {GetTableName()} " +
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

        public override void Delete()
        {
            int nrRowsAffected = ExecuteNonQuery($"DELETE FROM {GetTableName()} " +
                $"WHERE target_id='{TargetId}' AND overseer_id='{OverseerId}';");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to delete TrackingSettings (target_id = {TargetId}, overseer_id = {OverseerId}) from database.");
            }
        }

        public override void Update()
        {
            throw new NotImplementedException("Use the Save() method.");
        }

        public override void Save()
        {
            if (OverseerId == DEFAULT_ID || TargetId == DEFAULT_ID)
            {
                throw new DatabaseException("TrackingSettings composite key is missing a part.");
            }

            int nrRowsAffected = ExecuteNonQuery($"INSERT INTO {GetTableName()} (overseer_id, target_id, `interval`) " +
                $"VALUES ({OverseerId}, {TargetId}, {Interval}) " +
                $"ON DUPLICATE KEY UPDATE " +
                $"`interval` = {Interval};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to save TrackingSettings (target_id = {TargetId}, overseer_id = {OverseerId}) to database.");
            }
        }

        public static string GetTableName()
        {
            return "tracking_settings";
        }

        public override void LoadUsingAvailableData()
        {
            List<string> columnsToSelect = new List<string>(1);
            List<string> conditions = new List<string>(1);
            if (OverseerId != DEFAULT_ID) conditions.Add($"overseer_id = {OverseerId}");
            else columnsToSelect.Add($"overseer_id");
            if (TargetId != DEFAULT_ID) conditions.Add($"target_id = {TargetId}");
            else columnsToSelect.Add($"target_id");
            // doesn't make much sense, but whatever
            if (Interval != DEFAULT_INTERVAL) conditions.Add($"`interval` = {Interval}");
            else columnsToSelect.Add($"`interval`");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException("No data available to tracking settings by.");
            }

            string sql = $"SELECT {String.Join(", ", columnsToSelect)} " +
                $"FROM {GetTableName()} " +
                $"WHERE {String.Join(" AND ", conditions)} " +
                "LIMIT 2;";
            List<Dictionary<string, object>> results = ExecuteQuery(sql);

            if (results.Count() < 1)
            {
                throw new TableEntryDoesNotExistException("TrackingSettings not found in database.");
            }

            foreach (string columnSelected in columnsToSelect)
            {
                this[ColumnNameToPropertyName(CleanColumnName(columnSelected))] = results[0][CleanColumnName(columnSelected)];
            }
        }
    }
}
