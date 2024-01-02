using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class OverseerUser : DatabaseClient
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        // TrackedUserIds should always fill from left to right
        public long[] TrackedUserIds { get; private set; }

        static OverseerUser()
        {
            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {GetTableName()} " +
                    $"(id INTEGER NOT NULL AUTO_INCREMENT, " +
                    $"email VARCHAR(50) NOT NULL, " +
                    $"name VARCHAR(50), " +
                    $"password_hash VARCHAR(250) NOT NULL, " +
                    $"PRIMARY KEY (id), " +
                    $"UNIQUE (email)" +
                    $");";
                ExecuteNonQuery(sql);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
                Trace.TraceWarning(e.StackTrace);
            }
        }

        public static string GetTableName()
        {
            return "overseer_user";
        }

        public override void LoadUsingAvailableData()
        {
            List<string> columnsToSelect = new List<string>(1);
            List<string> conditions = new List<string>(1);
            if (Id != -1) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (Email != "") conditions.Add($"email = '{Email}'");
            else columnsToSelect.Add("email");
            if (Name != "") conditions.Add($"name = '{Name}'");
            else columnsToSelect.Add("name");
            if (PasswordHash != "") conditions.Add($"password_hash = '{PasswordHash}'");
            else columnsToSelect.Add("password_hash");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException("No data available to loead overseer user by.");
            }

            List<Dictionary<string, object>> results = ExecuteQuery($"SELECT {String.Join(", ", columnsToSelect)} " +
                $"FROM {GetTableName()} " +
                $"WHERE {String.Join(" AND ", conditions)} " +
                "LIMIT 2;");

            if (results.Count() < 1)
            {
                throw new TableEntryDoesNotExistException("Overseer user not found in database.");
            }

            foreach (string columnSelected in columnsToSelect)
            {
                this[ColumnNameToPropertyName(columnSelected)] = results[0][columnSelected];
            }

            LoadTrackedUserIds();
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != -1)
            {
                throw new TableEntryAlreadyExistsException("Overseer already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {GetTableName()} (email, name, password_hash) " +
                $"VALUES ('{Email}', '{Name}', '{PasswordHash}')");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add overseer (id = {Id}) to database.");
            }
            this.Id = OverseerUser.LastInsertedId;
            TrackedUserId.SyncOverseer(this);
        }

        public override void Update()
        {
            if (Id == -1)
            {
                throw new TableEntryDoesNotExistException($"Overseer user id to update was -1.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {GetTableName()} " +
                $"SET email = '{Email}', name = '{Name}', password_hash = '{PasswordHash}' " +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update overseer (id = {Id}) in database.");
            }
            TrackedUserId.SyncOverseer(this);
        }

        public override void Save()
        {
            if (Id == -1)
            {
                Add();
            }
            else
            {
                Update();
            }
        }

        public override void Delete()
        {
            int nrRowsAffected = ExecuteNonQuery($"DELETE FROM {GetTableName()} " +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to delete overseer (id = {Id}) from database.");
            }
            TrackedUserId.DeleteByOverseer(Id);
        }
    }
}
