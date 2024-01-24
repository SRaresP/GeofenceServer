using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class OverseerUser : DatabaseClient
    {
        new public static string TableName => "overseer_user";
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        // TrackedUserIds should always fill from left to right
        public long[] TrackedUserIds { get; private set; }

        static OverseerUser()
        {
            string sql = $"CREATE TABLE IF NOT EXISTS {TableName} " +
                $"(id INTEGER NOT NULL AUTO_INCREMENT, " +
                $"email VARCHAR(50) NOT NULL, " +
                $"name VARCHAR(50), " +
                $"password_hash VARCHAR(250) NOT NULL, " +
                $"PRIMARY KEY (id), " +
                $"UNIQUE (email)" +
                $");";
            ExecuteNonQuery(sql);
        }

        protected override void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != DEFAULT_ID) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (Email != "") conditions.Add($"email = '{Email}'");
            else columnsToSelect.Add("email");
            if (Name != "") conditions.Add($"name = '{Name}'");
            else columnsToSelect.Add("name");
            if (PasswordHash != "") conditions.Add($"password_hash = '{PasswordHash}'");
            else columnsToSelect.Add("password_hash");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException($"No data available to load {GetType().Name}s by.");
            }
        }

        public override bool LoadUsingAvailableData()
        {
            bool success = base.LoadUsingAvailableData();
            if (success)
            {
                LoadTrackedUserIds();
            }
            return success;
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException($"{GetType().Name} already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (email, name, password_hash) " +
                $"VALUES ('{Email}', '{Name}', '{PasswordHash}')");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add {GetType().Name} (id = {Id}) to database.");
            }
            this.Id = OverseerUser.LastInsertedId;
            TrackedUserId.SyncOverseer(this);
        }

        public override void Update()
        {
            if (Id == DEFAULT_ID)
            {
                throw new TableEntryDoesNotExistException($"{GetType().Name}  id to update was {DEFAULT_ID}.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {TableName} " +
                $"SET email = '{Email}', name = '{Name}', password_hash = '{PasswordHash}' " +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update {GetType().Name} (id = {Id}) in database.");
            }
            TrackedUserId.SyncOverseer(this);
        }

        public override void Save()
        {
            if (Id == DEFAULT_ID)
            {
                Add();
            }
            else
            {
                Update();
            }
        }

        public override int Delete()
        {
            int nrRowsAffected = ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE id = {Id};");
            if (nrRowsAffected > 0)
            {
                TrackedUserId.DeleteByOverseer(Id);
            }
            return nrRowsAffected;
        }

        public override bool IsLoaded()
        {
            return Id != DEFAULT_ID;
        }
    }
}
