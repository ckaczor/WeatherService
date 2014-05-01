using System.Collections.Generic;
using WeatherService.Values;

namespace WeatherService.Remote
{
    public class DeviceHistory : Dictionary<WeatherValueType, List<ReadingBase>>
    {
    }
}
