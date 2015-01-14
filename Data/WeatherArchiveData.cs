using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;

namespace WeatherService.Data
{
    public class WeatherArchiveData : DbContext
    {
        private const string ConnectionStringTemplateName = "WeatherArchiveData";
        private const string DatabaseNameYearTemplate = "WeatherData{0}";

        private static readonly Dictionary<int, bool> DatabaseExists = new Dictionary<int, bool>();

        private static string BuildConnectionString(int year)
        {
            var databaseName = string.Format(DatabaseNameYearTemplate, year);

            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringTemplateName].ConnectionString;

            var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = databaseName };

            return builder.ConnectionString;
        }

        public WeatherArchiveData(int year)
            : base(BuildConnectionString(year))
        {
            if (DatabaseExists.ContainsKey(year))
                return;

            DatabaseExists[year] = Database.Exists();

            if (DatabaseExists[year])
                return;

            Database.Create();

            DatabaseExists[year] = true;
        }

        public virtual DbSet<Reading> Readings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
