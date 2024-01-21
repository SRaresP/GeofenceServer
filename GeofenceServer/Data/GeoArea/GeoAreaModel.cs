using System;
using System.Collections.Generic;
using System.Linq;

namespace GeofenceServer.Data
{
    public partial class GeoArea : DatabaseClient
    {

        public long Id { get; set; } = DEFAULT_ID;
        public long OverseerId { get; set; } = DEFAULT_ID;
        public long TargetId { get; set; } = DEFAULT_ID;
        public int Color { get; set; } = DEFAULT_COLOR;
        public GeoAreaMode Mode { get; set; } = GeoAreaMode.NONE;
        public string TriggerMessage { get; set; } = "";

        static GeoArea()
        {
            int result = ExecuteNonQuery(
                $"CREATE TABLE IF NOT EXISTS {TableName} " +
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

        public override int Delete()
        {
            return ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE id='{Id}';");
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException($"{GetType().Name} already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (overseer_id, target_id, color, mode, trigger_message) " +
                $"VALUES ({OverseerId}, {TargetId}, {Color}, {(int)Mode}, '{TriggerMessage}')");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add {GetType().Name} (id = {Id}) to database.");
            }
            this.Id = GeoArea.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == DEFAULT_ID)
            {
                throw new TableEntryDoesNotExistException($"{GetType().Name} id to update was {DEFAULT_ID}.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {TableName} " +
                $"SET overseer_id = {OverseerId}, " +
                $"target_id = {TargetId}, " +
                $"color = {Color}, " +
                $"mode = {(int)Mode}, " +
                $"trigger_message = '{TriggerMessage}'" +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update {GetType().Name} (id = {Id}) in database.");
            }
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
        new public static string TableName => "geo_area";

        protected override void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != DEFAULT_ID) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (OverseerId != DEFAULT_ID) conditions.Add($"overseer_id = {OverseerId}");
            else columnsToSelect.Add($"overseer_id");
            if (TargetId != DEFAULT_ID) conditions.Add($"target_id = {TargetId}");
            else columnsToSelect.Add($"target_id");
            if (Color != DEFAULT_COLOR) conditions.Add($"color = {Color}");
            else columnsToSelect.Add($"color");
            if (Mode != GeoAreaMode.NONE) conditions.Add($"mode = {(int)Mode}");
            else columnsToSelect.Add($"mode");
            if (TriggerMessage != "") conditions.Add($"trigger_message = {TriggerMessage}");
            else columnsToSelect.Add($"trigger_message");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException($"No data available to select {GetType().Name} by.");
            }
        }
    }
}
