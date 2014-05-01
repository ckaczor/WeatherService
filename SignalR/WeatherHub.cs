using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace WeatherService.SignalR
{
    public class WeatherHub : Hub
    {
        public void Send(string message)
        {
            Clients.Others.addMessage(message);
        }
    }
}
