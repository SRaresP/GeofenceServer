using System;
using System.Collections.Generic;
using System.Linq;

namespace GeofenceServer.Data
{
    public partial class GeoArea : DatabaseClient
    {

        public long Id { get; set; }
        public long OverseerId { get; set; }
        public long TargetId { get; set; }
        public int Color { get; set; }
        public GeoAreaMode Mode { get; set; }
        public string TriggerMessage { get; set; }

        static GeoArea()
        {
            int result = ExecuteNonQuery(
                $"CREATE TABLE IF NOT EXISTS {GetTableName()} " +
                $"(id BIGINT NOT NULL AUTO_INCREMENT, " +
                $"overseer_id BIGINT NOT NULL, " +
                $"target_id BIGINT NOT NULL, " +
                $"color INTEGER NOT NULL, " +
                $"mode INTEGER NOT NULL, " +
                $"trigger_message VARCHAR(5000) NOT NULL, " +
                $"PRIMARY KEY (id)," +
                $"FOREIGN KEY(overseer_id) REFERENCES overseer_user(id), " +
                $"FOREIGN KEY(target_id) REFERENCES target_user(id)" +
                $");");
        }

        public override void Delete()
        {
            int nrRowsAffected = ExecuteNonQuery($"DELETE FROM {GetTableName()} " +
                $"WHERE id='{Id}';");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to delete GeoArea (id = {Id}) from database.");
            }
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != -1)
            {
                throw new TableEntryAlreadyExistsException("Overseer already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {GetTableName()} (overseer_id, target_id, color, mode, trigger_message) " +
                $"VALUES ({OverseerId}, {TargetId}, {Color}, {(int)Mode}, '{TriggerMessage}')");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add GeoArea (id = {Id}) to database.");
            }
            this.Id = GeoArea.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == -1)
            {
                throw new TableEntryDoesNotExistException($"GeoArea id to update was -1.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {GetTableName()} " +
                $"SET overseer_id = {OverseerId}, " +
                $"target_id = {TargetId}, " +
                $"color = {Color}, " +
                $"mode = {(int)Mode}, " +
                $"trigger_message = '{TriggerMessage}'" +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update GeoArea (id = {Id}) in database.");
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

        public static string GetTableName()
        {
            return "geo_area";
        }

        protected void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != -1) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (OverseerId != -1) conditions.Add($"overseer_id = {OverseerId}");
            else columnsToSelect.Add($"overseer_id");
            if (TargetId != -1) conditions.Add($"target_id = {TargetId}");
            else columnsToSelect.Add($"target_id");
            if (Color != DEFAULT_COLOR) conditions.Add($"color = {Color}");
            else columnsToSelect.Add($"color");
            if (Mode != GeoAreaMode.NONE) conditions.Add($"mode = {(int)Mode}");
            else columnsToSelect.Add($"mode");
            if (TriggerMessage != "") conditions.Add($"trigger_message = {TriggerMessage}");
            else columnsToSelect.Add($"trigger_message");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException("No data available to select GeoArea by.");
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
                throw new TableEntryDoesNotExistException("GeoArea not found in database.");
            }

            foreach (string columnSelected in columnsToSelect)
            {
                this[ColumnNameToPropertyName(CleanColumnName(columnSelected))] = results[0][CleanColumnName(columnSelected)];
            }
        }

        public GeoArea[] LoadMultipleUsingAvailableData()
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
                throw new TableEntryDoesNotExistException("No GeoArea found in database.");
            }

            GeoArea[] geoAreas = new GeoArea[resultCount];
            for (int geoAreaIndex = 0; geoAreaIndex < resultCount; ++geoAreaIndex)
            {
                geoAreas[geoAreaIndex] = new GeoArea(this);
                for (int columnIndex = 0; columnIndex < columnsToSelect.Count(); ++columnIndex)
                {
                    string cleanColName = CleanColumnName(columnsToSelect[columnIndex]);
                    geoAreas[geoAreaIndex][ColumnNameToPropertyName(cleanColName)] = results[0][cleanColName];
                }
            }
            return geoAreas;
        }
    }
}
