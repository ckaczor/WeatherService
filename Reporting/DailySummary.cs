using System;

namespace WeatherService.Reporting
{
    public class DailySummary
    {
        public DateTimeOffset? Date { get; set; }
        public int Count { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Average { get; set; }
    }
}
