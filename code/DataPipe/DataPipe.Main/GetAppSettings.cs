using System.IO;

using Microsoft.Extensions.Configuration;

namespace DataPipe.Main
{
    public static class GetAppSettings
    {
        public static AppSettings Get()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            AppSettings settings = new AppSettings();
            configuration.Bind(settings);
            return settings;
        }
}
}