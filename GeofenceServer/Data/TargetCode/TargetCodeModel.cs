﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class TargetCode : DatabaseClient
    {
        new public static string TableName => "target_code";
        public long Id { get; set; }
        public long TargetUserId { get; set; }
        public string Code { get; set; }

        static TargetCode()
        {
            string sql = $"CREATE TABLE IF NOT EXISTS {TableName} " +
                $"(id BIGINT NOT NULL AUTO_INCREMENT, " +
                $"target_user_id BIGINT NOT NULL, " +
                $"code CHAR(8) NOT NULL, " +
                $"PRIMARY KEY (id), " +
                $"FOREIGN KEY(target_user_id) REFERENCES target_user(id)" +
                $");";
            ExecuteNonQuery(sql);
        }

        protected override void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != DEFAULT_ID) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (TargetUserId != DEFAULT_ID) conditions.Add($"target_user_id = {TargetUserId}");
            else columnsToSelect.Add("target_user_id");
            if (Code != "") conditions.Add($"code = '{Code}'");
            else columnsToSelect.Add("code");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException($"No data available to load {GetType().Name}s by.");
            }
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != TrackedUserId.DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException($"{GetType().Name} already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (target_user_id, code) " +
                $"VALUES ({TargetUserId}, '{Code}')");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add {GetType().Name} (id = {Id}) to database.");
            }
            this.Id = TargetCode.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == DEFAULT_ID)
            {
                throw new TableEntryDoesNotExistException($"{GetType().Name} id to update was {DEFAULT_ID}.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {TableName} " +
                $"SET target_user_id = {TargetUserId}, code = '{Code}' " +
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

        public override int Delete()
        {
            return ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE id = {Id};");
        }

        public override bool IsLoaded()
        {
            return Id != DEFAULT_ID;
        }
    }
}
