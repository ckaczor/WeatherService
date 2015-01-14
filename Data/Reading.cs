using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherService.Data
{
    [Table("Reading")]
    public class Reading
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public int Type { get; set; }

        public double Value { get; set; }

        public DateTimeOffset ReadTime { get; set; }
    }
}
