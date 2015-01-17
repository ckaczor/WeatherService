using Common.Debug;
using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;
using System.ServiceModel.Web;
using System.ServiceProcess;
using WeatherService.Framework;
using WeatherService.Remote;
using WeatherService.SignalR;

namespace WeatherService
{
    [WindowsService("WeatherReporting", DisplayName = "Weather Reporting", Description = "", StartMode = ServiceStartMode.Automatic, ServiceAccount = ServiceAccount.LocalSystem)]
    public class ServiceImplementation : IWindowsService
    {
        private WebServiceHost _serviceHost;
        private IDisposable _signalR;

        public void OnStart(string[] args)
        {
            using (new BeginEndTracer(GetType().Name))
            {
                try
                {
                    _serviceHost = new WebServiceHost(typeof(WeatherServiceDuplex));
                    _serviceHost.Open();

                    _signalR = WebApp.Start<Startup>(Settings.Default.SignalR_ListenUrl);
                    Trace.Listeners.Remove("HostingTraceListener");

                    Program.Session = new Session();

                    Program.Session.Initialize();
                    Program.Session.StartRefresh();
                }
                catch (Exception exception)
                {
                    Tracer.WriteException("ServiceImplementation.OnStart", exception);
                    throw;
                }
            }
        }

        public void OnStop()
        {
            using (new BeginEndTracer(GetType().Name))
            {
                try
                {
                    Program.Session.StopRefresh();
                    Program.Session.Terminate();

                    _signalR.Dispose();

                    _serviceHost.Close();
                }
                catch (Exception exception)
                {
                    Tracer.WriteException("ServiceImplementation.OnStop", exception);
                    throw;
                }
            }
        }

        public void OnPause() { }

        public void OnContinue() { }

        public void OnShutdown() { }

        public void Dispose() { }

        public void OnCustomCommand(int command) { }
    }
}
