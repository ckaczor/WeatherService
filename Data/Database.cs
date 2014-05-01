using Common.Debug;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlServerCe;
using System.Linq;
using WeatherService.Values;

namespace WeatherService.Data
{
    /// <summary>
    /// This class manages database connectivity and logging
    /// </summary>
    internal static class Database
    {
        private static DatabaseContext _databaseContext;
        private static SqlCeConnection _connection;

        public static void Connect(string databasePath)
        {
            _connection = new SqlCeConnection(string.Format("Data Source={0}", databasePath));
            _connection.Open();
        }

        public static SqlCeConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        public static void Initialize(string databasePath)
        {
            Connect(databasePath);

            // Create a database context
            _databaseContext = new DatabaseContext(databasePath);

            // Turn on logging if requested
            if (Settings.Default.LogDatabase)
                _databaseContext.Log = Tracer.Writer;

            // Create the database engine
            SqlCeEngine engine = new SqlCeEngine(string.Format("Data Source={0}", databasePath));

            // Check to see if the database exists
            if (!_databaseContext.DatabaseExists())
            {
                // Create the database itself
                engine.CreateDatabase();

                // Run the creation script                
                // executeScript(Resources.CreateDatabase);
            }

            // Compact the database
            engine.Shrink();
        }

        public static Table<DeviceData> DeviceTable
        {
            get { return _databaseContext.DeviceTable; }
        }

        public static Table<ReadingData> ReadingTable
        {
            get { return _databaseContext.ReadingTable; }
        }

        public static IEnumerable<DeviceData> DeviceList
        {
            get { return from device in DeviceTable select device; }
        }

        public static IEnumerable<ReadingData> ReadingList(int deviceId)
        {
            return from reading in ReadingTable where reading.DeviceId == deviceId select reading;
        }

        public static IEnumerable<ReadingData> ReadingList(int deviceId, WeatherValueType valueType)
        {
            return from reading in ReadingTable where reading.DeviceId == deviceId && reading.Type == valueType select reading;
        }

        public static void SaveChanges()
        {
            _databaseContext.SubmitChanges(ConflictMode.ContinueOnConflict);
        }

        public static void DiscardChanges()
        {
            ChangeSet changeSet = _databaseContext.GetChangeSet();

            _databaseContext.Refresh(RefreshMode.OverwriteCurrentValues, changeSet.Inserts);
            _databaseContext.Refresh(RefreshMode.OverwriteCurrentValues, changeSet.Deletes);
            _databaseContext.Refresh(RefreshMode.OverwriteCurrentValues, changeSet.Updates);
        }
    }
}