using Common.Debug;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel.Web;
using System.ServiceProcess;
using WeatherService.Framework;
using WeatherService.SignalR;

namespace WeatherService
{
    [WindowsService("WeatherReporting", DisplayName = "Weather Reporting", Description = "", StartMode = ServiceStartMode.Automatic, ServiceAccount = ServiceAccount.LocalSystem)]
    public class ServiceImplementation : IWindowsService
    {
        private List<WebServiceHost> _serviceHosts;
        private IDisposable _signalR;

        public void OnStart(string[] args)
        {
            using (new BeginEndTracer(GetType().Name))
            {
                try
                {
                    _serviceHosts = new List<WebServiceHost>
                    {
                        new WebServiceHost(typeof (WeatherService)),
                        new WebServiceHost(typeof (WeatherServiceDuplex))
                    };

                    _serviceHosts.ForEach(h => h.Open());

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

                    _serviceHosts.ForEach(h => h.Close());
                    _serviceHosts.Clear();
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
