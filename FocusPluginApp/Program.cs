using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FocusPluginApp.Core;
using FocusPluginApp.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PluginInterface;


var builder = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory()) 
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<FocusMonitorService>();
    })
    .ConfigureContainer<ContainerBuilder>((context, container) =>
    {
        IConfiguration configuration = context.Configuration;

        
        var pluginMappings = configuration.GetSection("PluginMappings").Get<Dictionary<string, string>>() ?? new();
        string pluginDirectory = configuration.GetValue<string>("PluginDirectory") ?? "Plugins";

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation(" Loading plugins from: " + pluginDirectory);

        if (!Directory.Exists(pluginDirectory))
        {
            logger.LogWarning($" Plugin directory does not exist: {pluginDirectory}");
            //Directory.CreateDirectory(pluginDirectory);
        }

        
        var pluginLoader = new PluginLoader(pluginDirectory, pluginMappings, logger);
        container.RegisterInstance(pluginLoader).AsSelf().SingleInstance();

        var plugins = pluginLoader.LoadAllPlugins(); // Load all available plugins at startup
        foreach (var plugin in plugins)
        {
            container.RegisterInstance(plugin.Value).As<IPlugin>().Keyed<IPlugin>(plugin.Key);
            logger.LogInformation($" Registered Plugin: {plugin.Key} -> {plugin.Value.GetType().Name}");
        }
    });

var host = builder.Build();
await host.RunAsync();
