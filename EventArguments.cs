using System;
using WeatherService.Devices;

namespace WeatherService
{
    #region DeviceRefreshed

    [Serializable]
    public class DeviceRefreshedEventArgs : EventArgs
    {
        private readonly DeviceBase _device;

        public DeviceRefreshedEventArgs(DeviceBase Device)
        {
            _device = Device;
        }

        public DeviceBase Device
        {
            get
            {
                return _device;
            }
        }
    }

    #endregion
}
