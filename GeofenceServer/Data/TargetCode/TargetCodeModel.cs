using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class TargetCode : DatabaseClient
    {
        public long Id { get; set; }
        public long TargetUserId { get; set; }
        public string Code { get; set; }

        static TargetCode()
        {
            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {GetTableName()} " +
                    $"(id BIGINT NOT NULL AUTO_INCREMENT, " +
                    $"target_user_id BIGINT NOT NULL, " +
                    $"code CHAR(8) NOT NULL, " +
                    $"PRIMARY KEY (id), " +
                    $"FOREIGN KEY(target_user_id) REFERENCES target_user(id)" +
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
            return "target_code";
        }

        protected void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != -1) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (TargetUserId != -1) conditions.Add($"target_user_id = {TargetUserId}");
            else columnsToSelect.Add("target_user_id");
            if (Code != "") conditions.Add($"code = '{Code}'");
            else columnsToSelect.Add("code");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException("No data available to load TargetCodes by.");
            }
        }

        public override void LoadUsingAvailableData()
        {
            List<string> columnsToSelect = new List<string>(1);
            List<string> conditions = new List<string>(1);
            AddConditionsAndSelects(conditions, columnsToSelect);

            string sql = $"SELECT {String.Join(", ", columnsToSelect)} " +
                $"FROM {GetTableName()} " +
                $"WHERE {String.Join(" AND ", conditions)} " +
                "LIMIT 2;";
            List<Dictionary<string, object>> results = ExecuteQuery(sql);

            if (results.Count() < 1)
            {
                throw new TableEntryDoesNotExistException("TargetCode not found in database.");
            }

            foreach (string columnSelected in columnsToSelect)
            {
                this[ColumnNameToPropertyName(CleanColumnName(columnSelected))] = results[0][CleanColumnName(columnSelected)];
            }
        }

        public TargetCode[] LoadMultipleUsingAvailableData()
        {
            List<string> columnsToSelect = new List<string>(1);
            List<string> conditions = new List<string>(1);
            AddConditionsAndSelects(conditions, columnsToSelect);

            string sql = $"SELECT {String.Join(", ", columnsToSelect)} " +
                $"FROM {GetTableName()} " +
                $"WHERE {String.Join(" AND ", conditions)};";
            List<Dictionary<string, object>> results = ExecuteQuery(sql);

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                throw new TableEntryDoesNotExistException("No TargetCode found in database.");
            }

            TargetCode[] TargetCodes = new TargetCode[resultCount];
            for (int TargetCodeIndex = 0; TargetCodeIndex < resultCount; ++TargetCodeIndex)
            {
                TargetCodes[TargetCodeIndex] = new TargetCode(this);
                for (int columnIndex = 0; columnIndex < columnsToSelect.Count(); ++columnIndex)
                {
                    string cleanColName = CleanColumnName(columnsToSelect[columnIndex]);
                    TargetCodes[TargetCodeIndex][ColumnNameToPropertyName(cleanColName)] = results[TargetCodeIndex][cleanColName];
                }
            }
            return TargetCodes;
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != TrackedUserId.DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException("Target already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {GetTableName()} (target_user_id, code) " +
                $"VALUES ({TargetUserId}, '{Code}')");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add TargetCode (id = {Id}) to database.");
            }
            this.Id = TargetCode.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == -1)
            {
                throw new TableEntryDoesNotExistException($"TargetCode id to update was -1.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {GetTableName()} " +
                $"SET target_user_id = {TargetUserId}, code = '{Code}' " +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update TargetCode (id = {Id}) in database.");
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
