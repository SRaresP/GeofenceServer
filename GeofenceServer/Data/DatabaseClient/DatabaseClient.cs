using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GeofenceServer.Data
{
    public abstract class DatabaseClient
    {
        protected static MySqlConnection Connection;
        protected static string ConnectionCredentials;
        protected static long LastInsertedId;
        static DatabaseClient()
        {
            string dbAddress = ConfigurationManager.AppSettings.Get("database_host");
            string dbUser = ConfigurationManager.AppSettings.Get("database_user");
            string dbPassword = ConfigurationManager.AppSettings.Get("database_password");
            string dbName = ConfigurationManager.AppSettings.Get("database_name");

            if (dbAddress == null ||
                dbUser == null ||
                dbPassword == null ||
                dbName == null)
            {
                throw new MissingConfigurationException("Missing at least one database configuration string in configuration file (App.config).");
            }

            ConnectionCredentials =
                $"Server={dbAddress};" +
                $"User ID={dbUser};" +
                $"Password={dbPassword};" +
                $"Database={dbName}";

            // I don't think this thing ever throws any errors I can check, but if you see this I was probably wrong
            Connection = new MySqlConnection(ConnectionCredentials);
        }

        public object this[string propertyName]
        {
            get
            {
                PropertyInfo propInfo = GetType().GetProperty(propertyName);
                return propInfo.GetValue(this, null);
            }
            set
            {
                PropertyInfo propInfo = GetType().GetProperty(propertyName);
                propInfo.SetValue(this, value, null);
            }
        }

        public static List<Dictionary<string, object>> ExecuteQuery(string sql)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }
            MySqlCommand command = new MySqlCommand(sql, Connection);
            MySqlDataReader reader = command.ExecuteReader();
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (!reader.Read())
            {
                if (Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
                return new List<Dictionary<string, object>>(0);
            }
            ReadOnlyCollection<DbColumn> columnSchemas = reader.GetColumnSchema();
            int columnCount = columnSchemas.Count();
            Dictionary<string, object> row = new Dictionary<string, object>(columnCount);
            for (int colI = 0; colI < columnCount; ++colI)
            {
                row[columnSchemas[colI].BaseColumnName] = reader.GetValue(colI);
            }
            result.Add(row);

            while (reader.Read())
            {
                row = new Dictionary<string, object>(columnCount);
                for (int colI = 0; colI < columnCount; ++colI)
                {
                     row[columnSchemas[colI].BaseColumnName] = reader.GetValue(colI);
                }
                result.Add(row);
            }

            if (Connection.State != System.Data.ConnectionState.Closed)
            {
                Connection.Close();
            }
            return result;
        }

        public static int ExecuteNonQuery(string sql)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }
            MySqlCommand command = new MySqlCommand(sql, Connection);
            int nrRowsAffected = command.ExecuteNonQuery();
            List<object> result = new List<object>();
            if (Connection.State != System.Data.ConnectionState.Closed)
            {
                Connection.Close();
            }
            DatabaseClient.LastInsertedId = command.LastInsertedId;
            return nrRowsAffected;
        }

        public static string CleanColumnName(string column)
        {
            // columns may contain these in order to avoid conflicts with keywords, like the interval column in TrackingSettings
            return Regex.Replace(column, "['`\"]", "");
        }

        public DatabaseClient() { }

        public DatabaseClient(DatabaseClient toCopy)
        {
            FieldInfo[] classFields = GetType().GetFields(
              BindingFlags.NonPublic |
              BindingFlags.Public |
              BindingFlags.Instance);

            foreach (FieldInfo fieldInfo in classFields)
            {
                fieldInfo.SetValue(this, fieldInfo.GetValue(toCopy));
            }
        }

        public string PropertyNameToColumnName(string propertyName)
        {
            // This is Pascal Case to snake_case
            string temp = Regex.Replace(propertyName, "[^^]([A-Z])", (hit) => "_" + hit.ToString().ToLower());
            temp = Regex.Replace(temp, "^[A-Z]", (hit) => hit.ToString().ToLower());
            return temp;
        }

        public string ColumnNameToPropertyName(string columnName)
        {
            // This is snake_case to Pascal Case
            return Regex.Replace(columnName, "(^|_)(.?)", match => match.Groups[2].Value.ToUpper());
        }

        public abstract void Add();
        public abstract void Update();
        public abstract void Save();
        public abstract void Delete();
        public abstract void LoadUsingAvailableData();
        // ceva_super_tare
        // CevaSuperTare
    }
}
