using System;
using System.Threading;

using Autofac;

using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Scaffold;
using NLog;
using NLog.Config;


namespace DataReceiver.Main
{
    public class Program
    {
        private static readonly Logger _logger = LogManager.GetLogger("Data");

        private static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");

            const string mutexId = @"Global\{{183C889E-F9EF-4E47-A452-D3EB5C4D58B1}}";

            bool createdNew;
            using (var mutex = new Mutex(false, mutexId, out createdNew))
            {
                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(5000, false);
                        if (!hasHandle)
                        {
                            _logger.Error("Timeout waiting for exclusive access");
                            throw new TimeoutException("Timeout waiting for exclusive access");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        hasHandle = true;
                    }

                    try
                    {
                        _logger.Info("Starting up.");
                        var container = AutoFacBootstrapper.Init();

                        var worker = container.Resolve<ISetupReceivers>();
                        worker.DoWork();

                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Exception trying to get data {e.Message}");
                        throw;
                    }
                }
                finally
                {
                    if (hasHandle)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }
    }
}