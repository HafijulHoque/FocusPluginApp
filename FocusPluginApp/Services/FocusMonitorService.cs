using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using FocusPluginApp.Core;
using PluginInterface;

namespace FocusPluginApp.Services
{
    public class FocusMonitorService : BackgroundService
    {
        private readonly PluginLoader _pluginLoader;
        private readonly ILogger<FocusMonitorService> _logger;
        private string? _lastFocusedApp;

        public FocusMonitorService(PluginLoader pluginLoader, ILogger<FocusMonitorService> logger)
        {
            _pluginLoader = pluginLoader;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Focus Monitor Service Started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                string? activeApp = FocusDetector.GetActiveProcessName();

                if (!string.IsNullOrEmpty(activeApp) && activeApp != _lastFocusedApp)
                {
                    _logger.LogInformation($"Active Application: {activeApp}");

                    var plugin = _pluginLoader.GetPluginForProcess(activeApp);
                    if (plugin != null)
                    {
                        _logger.LogInformation($"Executing Plugin: {plugin.GetType().Name} for {activeApp}");
                        plugin.Execute();
                    }
                    else
                    {
                        _logger.LogWarning($"⚠ No plugin found for {activeApp}");
                    }

                    _lastFocusedApp = activeApp;
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
