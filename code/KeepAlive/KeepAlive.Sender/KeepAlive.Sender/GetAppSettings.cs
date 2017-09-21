using System.IO;
using Microsoft.Extensions.Configuration;

namespace KeepAlive.Sender
{
    public static class GetAppSettings
    {
        public static AppSettings Get()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var settings = new AppSettings();
            configuration.Bind(settings);
            return settings;
        }
    }
}