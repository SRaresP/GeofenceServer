using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GeofenceServer.Data
{
    public partial class GeoFence : DatabaseClient
    {
        public long Id { get; set; } = DEFAULT_ID;
        public long GeoAreaId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; }

        static GeoFence()
        {
            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {TableName} " +
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
        new public static string TableName => "geo_fence";

        protected override void AddConditionsAndSelects(List<string> conditions, List<string> columnsToSelect)
        {
            if (Id != DEFAULT_ID) conditions.Add($"id = {Id}");
            else columnsToSelect.Add($"id");
            if (GeoAreaId != DEFAULT_ID) conditions.Add($"geo_area_id = {GeoAreaId}");
            else columnsToSelect.Add("geo_area_id");
            if (Latitude != DEFAULT_COORD) conditions.Add($"latitude = {Latitude}");
            else columnsToSelect.Add("latitude");
            if (Longitude != DEFAULT_COORD) conditions.Add($"longitude = '{Longitude}'");
            else columnsToSelect.Add("longitude");
            if (RadiusMeters != -1) conditions.Add($"radius_meters = '{RadiusMeters}'");
            else columnsToSelect.Add("radius_meters");

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

            nrOfRowsAffected = ExecuteNonQuery($"INSERT INTO {TableName} (geo_area_id, latitude, longitude, radius_meters) " +
                $"VALUES ({GeoAreaId}, {Latitude}, {Longitude}, {RadiusMeters})");
            if (nrOfRowsAffected < 1)
            {
                throw new DatabaseException($"Failed to add {GetType().Name} (id = {Id}) to database.");
            }
            this.Id = GeoFence.LastInsertedId;
        }

        public override void Update()
        {
            if (Id == DEFAULT_ID)
            {
                throw new TableEntryDoesNotExistException($"{GetType().Name} id to update was {DEFAULT_ID}.");
            }
            int nrRowsAffected = ExecuteNonQuery($"UPDATE {TableName} " +
                $"SET geo_area_id = {GeoAreaId}, latitude = {Latitude}, longitude = {Longitude}, radius_meters = {RadiusMeters} " +
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
    }
}
