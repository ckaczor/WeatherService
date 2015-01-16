using System.Data.Entity;

namespace WeatherService.Data
{
    public class WeatherData : DbContext
    {
        public WeatherData()
            : base("name=WeatherData")
        {
        }

        public virtual DbSet<DeviceData> Devices { get; set; }
        public virtual DbSet<SettingData> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
