using System;
using System.Collections.Generic;

namespace GeofenceServer.Data
{
    public partial class TrackedUserId : DatabaseClient
    {
        new public static string TableName => "tracked_user_id";
        public long TargetId;
        public long OverseerId;

        static TrackedUserId()
        {
            int result = ExecuteNonQuery(
                $"CREATE TABLE IF NOT EXISTS {TableName} " +
                $"(target_id INTEGER NOT NULL, " +
                $"overseer_id INTEGER NOT NULL, " +
                $"PRIMARY KEY (target_id, overseer_id), " +
                $"FOREIGN KEY(target_id) REFERENCES target_user(id), " +
                $"FOREIGN KEY(overseer_id) REFERENCES overseer_user(id) " +
                $");");
        }
        
        protected override void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (TargetId != DEFAULT_ID) conditions.Add($"target_id = {TargetId}");
            else columnsToSelect.Add($"target_id");
            if (OverseerId != DEFAULT_ID) conditions.Add($"overseer_id = '{OverseerId}'");
            else columnsToSelect.Add("overseer_id");

            if (conditions.Count < 1)
            {
                throw new DatabaseException($"No data available to load {this.GetType().Name}s by.");
            }
        }

        public override void Add()
        {
            if (OverseerId != DEFAULT_ID && TargetId != DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException($"This {GetType().Name} is already in the database. Cannot add it again.");
            }

            int nrRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (tracked_user_id, overseer_id) " +
                $"VALUES ({TargetId}, {OverseerId});");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add {GetType().Name} (target_id = {TargetId}, overseer_id = {OverseerId}) to database.");
            }
        }

        public override int Delete()
        {
            return ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE target_id='{TargetId}' AND overseer_id='{OverseerId}';");
        }

        public override void Save()
        {
            throw new NotImplementedException($"Save method doesn't make any sense for {GetType().Name}. Delete the {GetType().Name} when it has the desired id's, then change the id's, then call Add().");
        }

        public override void Update()
        {
            throw new NotImplementedException($"Update method doesn't make any sense for {GetType().Name}. Delete the {GetType().Name} when it has the desired id's, then change the id's, then call Add().");
        }

        public override bool IsLoaded()
        {
            return (TargetId != DEFAULT_ID) && (OverseerId != DEFAULT_ID);
        }
    }
}
