using System;
using System.Data.Linq.Mapping;
using System.Data.SqlTypes;
using WeatherService.Values;

namespace WeatherService.Data
{
    [Table(Name = "Reading")]
    public class ReadingData
    {
        public ReadingData()
        {
            ReadTime = SqlDateTime.MinValue.Value;
        }

        [Column(UpdateCheck = UpdateCheck.Never, IsDbGenerated = true, IsPrimaryKey = true)]
        public int Id
        {
            get;
            internal set;
        }

        [Column(UpdateCheck = UpdateCheck.Never)]
        public int DeviceId
        {
            get;
            internal set;
        }

        [Column(UpdateCheck = UpdateCheck.Never)]
        public WeatherValueType Type
        {
            get;
            internal set;
        }

        [Column(UpdateCheck = UpdateCheck.Never)]
        public double Value
        {
            get;
            internal set;
        }

        [Column(UpdateCheck = UpdateCheck.Never)]
        public DateTime ReadTime
        {
            get;
            internal set;
        }

        //private EntityRef<DeviceData> _device;
        //[Association(Storage = "_device", ThisKey = "DeviceId", OtherKey = "Id")]
        //public DeviceData Device
        //{
        //    get { return _device.Entity; }
        //    set { _device.Entity = value; }
        //}

    }
}
