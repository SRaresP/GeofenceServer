using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class GeoFence : DatabaseClient
    {
        public long Id { get; set; }
        public long GeoAreaId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; }

        static GeoFence()
        {
            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {GetTableName()} " +
                    $"(id BIGINT NOT NULL AUTO_INCREMENT, " +
                    $"geo_area_id BIGINT NOT NULL, " +
                    // FLOAT(8,5) => a float type of 8 total bits out of which 5 bits are used after the decimal point.
                    // Could've used (8,6) for latitude since it only ranges from -90 to 90, but decided to keep it consistent.
                    $"latitude FLOAT(8,5) NOT NULL, " +
                    $"longitude FLOAT(8,5) NOT NULL, " +
                    $"radius_meters INTEGER NOT NULL, " +
                    $"PRIMARY KEY (id), " +
                    $"FOREIGN KEY(geo_area_id) REFERENCES geo_area(id)" +
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
            return "geo_fence";
        }

        protected void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != -1) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (GeoAreaId != -1) conditions.Add($"geo_area_id = {GeoAreaId}");
            else columnsToSelect.Add("geo_area_id");
            if (Latitude != DEFAULT_COORD) conditions.Add($"latitude = {Latitude}");
            else columnsToSelect.Add("latitude");
            if (Longitude != DEFAULT_COORD) conditions.Add($"longitude = '{Longitude}'");
            else columnsToSelect.Add("longitude");
            if (RadiusMeters != -1) conditions.Add($"radius_meters = '{RadiusMeters}'");
            else columnsToSelect.Add("radius_meters");

            if (conditions.Count() < 1)
            {
                throw new DatabaseException("No data available to load GeoFences by.");
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
                throw new TableEntryDoesNotExistException("GeoFence not found in database.");
            }

            foreach (string columnSelected in columnsToSelect)
            {
                this[ColumnNameToPropertyName(CleanColumnName(columnSelected))] = results[0][CleanColumnName(columnSelected)];
            }
        }

        public GeoFence[] LoadMultipleUsingAvailableData()
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
                throw new TableEntryDoesNotExistException("No GeoFence found in database.");
            }

            GeoFence[] geoFences = new GeoFence[resultCount];
            for (int geoFenceIndex = 0; geoFenceIndex < resultCount; ++geoFenceIndex)
            {
                geoFences[geoFenceIndex] = new GeoFence(this);
                for (int columnIndex = 0; columnIndex < columnsToSelect.Count(); ++columnIndex)
                {
                    string cleanColName = CleanColumnName(columnsToSelect[columnIndex]);
                    geoFences[geoFenceIndex][ColumnNameToPropertyName(cleanColName)] = results[geoFenceIndex][cleanColName];
                }
            }
            return geoFences;
        }

        public override void Add()
        {
            int nrOfRowsAffected;
            if (Id != TrackedUserId.DEFAULT_ID)
            {
                throw new TableEntryAlreadyExistsException("Target already exists.");
            }

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {GetTableName()} (geo_area_id, latitude, longitude, radius_meters) " +
                $"VALUES ({GeoAreaId}, {Latitude}, {Longitude}, {RadiusMeters})");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add GeoFence (id = {Id}) to database.");
            }
            this.Id = GeoFence.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == -1)
            {
                throw new TableEntryDoesNotExistException($"Target user id to update was -1.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {GetTableName()} " +
                $"SET geo_area_id = {GeoAreaId}, latitude = {Latitude}, longitude = {Longitude}, radius_meters = {RadiusMeters} " +
                $"WHERE id = {Id};");
            if (nrRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to update GeoFence (id = {Id}) in database.");
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
