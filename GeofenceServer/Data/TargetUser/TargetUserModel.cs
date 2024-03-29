﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class TargetUser : DatabaseClient
    {
        new public static string TableName => "target_user";
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string LocationHistory { get; set; }
        public int NrOfCodeGenerations { get; set; }

        static TargetUser()
        {
            string sql = $"CREATE TABLE IF NOT EXISTS {TableName} " +
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
            if (LocationHistory != "") conditions.Add($"location_history = '{LocationHistory}'");
            else columnsToSelect.Add("location_history");
            if (NrOfCodeGenerations != DEFAULT_NR_CODE_GENS) conditions.Add($"nr_of_code_generations = '{NrOfCodeGenerations}'");
            else columnsToSelect.Add("nr_of_code_generations");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException($"No data available to load {this.GetType().Name}s by.");
            }
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != TrackedUserId.DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException($"{GetType().Name} already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (email, name, password_hash, location_history, nr_of_code_generations) " +
                $"VALUES ('{Email}', '{Name}', '{PasswordHash}', '{LocationHistory}', {NrOfCodeGenerations})");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add {GetType().Name} (id = {Id}) to database.");
            }
            this.Id = TargetUser.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == DEFAULT_ID)
            {
                throw new TableEntryDoesNotExistException($"{GetType().Name} id to update was {DEFAULT_ID}.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {TableName} " +
                $"SET email = '{Email}', name = '{Name}', password_hash = '{PasswordHash}', location_history = '{LocationHistory}', nr_of_code_generations = {NrOfCodeGenerations} " +
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
