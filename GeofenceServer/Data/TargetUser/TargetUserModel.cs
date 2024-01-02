using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class TargetUser : DatabaseClient
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string LocationHistory { get; set; }
        public int NrOfCodeGenerations { get; set; }

        static TargetUser()
        {
            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {GetTableName()} " +
                    $"(id BIGINT NOT NULL AUTO_INCREMENT, " +
                    $"email VARCHAR(50) NOT NULL, " +
                    $"name VARCHAR(50), " +
                    $"password_hash VARCHAR(250) NOT NULL, " +
                    $"location_history VARCHAR(5000), " +
                    $"nr_of_code_generations INTEGER, " +
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
            return "target_user";
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
            if (LocationHistory != "") conditions.Add($"location_history = '{LocationHistory}'");
            else columnsToSelect.Add("location_history");
            if (NrOfCodeGenerations != DEFAULT_NR_CODE_GENS) conditions.Add($"nr_of_code_generations = '{NrOfCodeGenerations}'");
            else columnsToSelect.Add("nr_of_code_generations");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException("No data available to load target user by.");
            }

            string sql = $"SELECT {String.Join(", ", columnsToSelect)} " +
                $"FROM {GetTableName()} " +
                $"WHERE {String.Join(" AND ", conditions)} " +
                "LIMIT 2;";
            List<Dictionary<string, object>> results = ExecuteQuery(sql);

            if (results.Count() < 1)
            {
                throw new TableEntryDoesNotExistException("Target user not found in database.");
            }

            foreach (string columnSelected in columnsToSelect)
            {
                this[ColumnNameToPropertyName(columnSelected)] = results[0][columnSelected];
            }
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != TrackedUserId.DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException("Target already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {GetTableName()} (email, name, password_hash, location_history, nr_of_code_generations) " +
                $"VALUES ('{Email}', '{Name}', '{PasswordHash}', '{LocationHistory}', {NrOfCodeGenerations})");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add target (id = {Id}) to database.");
            }
            this.Id = TargetUser.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == -1)
            {
                throw new TableEntryDoesNotExistException($"Target user id to update was -1.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {GetTableName()} " +
                $"SET email = '{Email}', name = '{Name}', password_hash = '{PasswordHash}', location_history = '{LocationHistory}', nr_of_code_generations = {NrOfCodeGenerations} " +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update target (id = {Id}) in database.");
            }
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
                throw new DatabaseException($"Failed to delete target (id = {Id}) from database.");
            }
        }
    }
}
