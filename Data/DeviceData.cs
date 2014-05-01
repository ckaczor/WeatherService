using System.Data.Linq.Mapping;

namespace WeatherService.Data
{
    [Table(Name = "Device")]
    public class DeviceData
    {
        [Column(UpdateCheck = UpdateCheck.Never, IsDbGenerated = true, IsPrimaryKey = true)]
        public int Id;

        [Column(UpdateCheck = UpdateCheck.Never)]
        public string Address;

        [Column(UpdateCheck = UpdateCheck.Never)]
        public string Name;

        [Column(UpdateCheck = UpdateCheck.Never)]
        public int ReadInterval;

        //private readonly EntitySet<Reading> _readingSet = new EntitySet<Reading>();
        //[Association(Storage = "_readingSet", OtherKey = "DeviceId", ThisKey = "Id")]
        //public EntitySet<Reading> Readings
        //{
        //    get { return _readingSet; }
        //    set { _readingSet.Assign(value); }
        //}
    }
}
