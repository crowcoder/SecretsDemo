﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SecretsDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

                        public static IWebHostBuilder ManualCreateDefaultBuilder(string[] args)
        {
            var builder = new WebHostBuilder();

            if (string.IsNullOrEmpty(builder.GetSetting(WebHostDefaults.ContentRootKey)))
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
            }

            if (args != null)
            {
                builder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
            }

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                if (env.IsDevelopment())
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                }

                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
            }).
            UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            });

            ConfigureWebDefaults(builder);

            return builder;
        }

        static void ConfigureWebDefaults(IWebHostBuilder builder)
        {
            builder.UseKestrel((builderContext, options) =>
            {
                options.Configure(builderContext.Configuration.GetSection("Kestrel"));
            })
            .ConfigureServices((hostingContext, services) =>
            {
                // Fallback
                services.PostConfigure<HostFilteringOptions>(options =>
                {
                    if (options.AllowedHosts == null || options.AllowedHosts.Count == 0)
                    {
                        // "AllowedHosts": "localhost;127.0.0.1;[::1]"
                        var hosts = hostingContext.Configuration["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        // Fall back to "*" to disable.
                        options.AllowedHosts = (hosts?.Length > 0 ? hosts : new[] { "*" });
                    }
                });
                // Change notification
                services.AddSingleton<IOptionsChangeTokenSource<HostFilteringOptions>>(
                            new ConfigurationChangeTokenSource<HostFilteringOptions>(hostingContext.Configuration));

                // services.AddTransient<IStartupFilter, HostFilteringStartupFilter>();
            });
            // .UseIIS()
            // .UseIISIntegration();
        }
    }
}