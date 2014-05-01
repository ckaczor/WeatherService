using System.Data.Linq;

namespace WeatherService.Data
{
    public class DatabaseContext : DataContext
    {
        public Table<DeviceData> DeviceTable;
        public Table<ReadingData> ReadingTable;

        public DatabaseContext(string databaseFile) : base(databaseFile) { }
    }
}
