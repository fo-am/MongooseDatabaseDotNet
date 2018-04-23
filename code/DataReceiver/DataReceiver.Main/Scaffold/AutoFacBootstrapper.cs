using Autofac;
using Autofac.Extras.NLog;

using DataReceiver.Main.Data;
using DataReceiver.Main.Handlers;
using DataReceiver.Main.Interfaces;

using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;

namespace DataReceiver.Main.Scaffold
{
    public class AutoFacBootstrapper
    {
        public static IContainer Init()
        {
            var appSettings = GetAppSettings.Get();

            var builder = new ContainerBuilder();
            builder.RegisterModule<NLogModule>();
            builder.RegisterType<SetupReceivers>().As<ISetupReceivers>();
            builder.RegisterType<Receiver>().As<IReceiver>();
            builder.RegisterType<PgRepository>().As<IPgRepository>();
            builder.RegisterType<GetHandler>().As<IGetHandler>();
            builder.Register<IConnection>(
                (ctx) =>
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = appSettings.RabbitHostName,
                        UserName = appSettings.RabbitUsername,
                        Password = appSettings.RabbitPassword,
                        RequestedHeartbeat = 15
                    };

                    var con = new AutorecoveringConnection(factory);
                    con.Init();
                    return con;
                }).SingleInstance();

            return builder.Build();
        }
    }
}