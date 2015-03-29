using System;
using System.Collections.Generic;
using System.ServiceModel;
using WeatherService.Devices;
using WeatherService.Values;

namespace WeatherService.Remote
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class WeatherServiceDuplex : IWeatherServiceDuplex
    {
        private static readonly List<IWeatherServiceCallback> Subscribers = new List<IWeatherServiceCallback>();

        public List<DeviceBase> GetDevices()
        {
            return WeatherServiceCommon.GetDevices();
        }

        public Dictionary<DeviceBase, List<ReadingBase>> GetGenericHistory(WeatherValueType valueType, DateTimeOffset start, DateTimeOffset end)
        {
            return WeatherServiceCommon.GetGenericHistory(valueType, start, end);
        }

        public Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes, DateTimeOffset start, DateTimeOffset end)
        {
            return WeatherServiceCommon.GetWindSpeedHistory(groupIntervalMinutes, start, end);
        }

        public Dictionary<string, int> GetWindDirectionHistory(DateTimeOffset start, DateTimeOffset end)
        {
            return WeatherServiceCommon.GetWindDirectionHistory(start, end);
        }

        public bool Subscribe()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IWeatherServiceCallback>();

                if (!Subscribers.Contains(callback))
                    Subscribers.Add(callback);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Unsubscribe()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IWeatherServiceCallback>();

                if (Subscribers.Contains(callback))
                    Subscribers.Remove(callback);

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void FireNotification(DeviceBase device)
        {
            // Create a list of callbacks that need to be removed
            var removeList = new List<IWeatherServiceCallback>();

            // Loop over each subscriber
            foreach (var callback in Subscribers)
            {
                // If the callback connection isn't open...
                if ((callback as ICommunicationObject).State != CommunicationState.Opened)
                {
                    // ...add it to the remove list and continue
                    removeList.Add(callback);
                    continue;
                }

                try
                {
                    // Make the callback
                    callback.OnDeviceUpdate(device);
                }
                catch
                {
                    // The callback failed - add it to the remove list
                    removeList.Add(callback);
                }

            }

            // Remove all callbacks in the remove list
            removeList.ForEach(o => Subscribers.Remove(o));
        }
    }
}
