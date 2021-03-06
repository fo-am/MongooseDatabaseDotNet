﻿using System.IO;

using Microsoft.Extensions.Configuration;

namespace DataInput.Core
{
    public static class GetAppSettings
    {
        public static AppSettings Get()
        {
            var builder = new ConfigurationBuilder()
               //   .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

            var configuration = builder.Build();

            AppSettings settings = new AppSettings();
            configuration.Bind(settings);
            return settings;
        }
    }
}