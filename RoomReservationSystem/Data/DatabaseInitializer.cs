using Dapper;
using Microsoft.Data.SqlClient;

namespace RoomReservationSystem.Data
{
    public static class DatabaseInitializer
    {
        public static void EnsureDatabaseExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string databaseName = builder.InitialCatalog;

            builder.InitialCatalog = "master";

            using (var masterConnection = new SqlConnection(builder.ConnectionString))
            {
                var checkDbSql = $"SELECT * FROM sys.databases WHERE name = '{databaseName}'";
                var dbExists = masterConnection.QueryFirstOrDefault(checkDbSql);

                if (dbExists == null)
                {
                    masterConnection.Execute($"CREATE DATABASE [{databaseName}]");
                }
            }

            using (var dbConnection = new SqlConnection(connectionString))
            {
                var checkTableSql = "SELECT * FROM sys.tables WHERE name = 'User'";
                var tableExists = dbConnection.QueryFirstOrDefault(checkTableSql);

                if (tableExists == null)
                {
                    var scriptPath = Path.Combine(AppContext.BaseDirectory, "init.sql");
                    if (File.Exists(scriptPath))
                    {
                        var script = File.ReadAllText(scriptPath);
                        dbConnection.Execute(script);
                    }
                }
            }
        }
    }
}