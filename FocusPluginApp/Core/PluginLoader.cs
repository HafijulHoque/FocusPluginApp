using System.Reflection;
using Microsoft.Extensions.Logging;
using PluginInterface;
namespace FocusPluginApp.Core
{
    public class PluginLoader
    {
        private readonly string _pluginDirectory;
        private readonly Dictionary<string, string> _pluginMappings;
        private readonly ILogger _logger;
        private readonly Dictionary<string, IPlugin> _loadedPlugins = new();

        public PluginLoader(string pluginDirectory, Dictionary<string, string> pluginMappings, ILogger logger)
        {
            _pluginDirectory = pluginDirectory;
            _pluginMappings = pluginMappings;
            _logger = logger;
        }

        public Dictionary<string, IPlugin> LoadAllPlugins()
        {
            _logger.LogInformation($"....Starting LoadAllPlugins()...The directory is:{_pluginDirectory}");

            if (!Directory.Exists(_pluginDirectory))
            {
                _logger.LogWarning($"plugin directory does not exist: {_pluginDirectory}");
                return new Dictionary<string, IPlugin>();
            }

            foreach (var file in Directory.GetFiles(_pluginDirectory, "*.dll"))
            {
                try
                {
                    _logger.LogInformation($"....Loading Assembly: {file}");
                    Assembly assembly = Assembly.LoadFrom(file);

                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface)
                        {
                            var pluginInstance = (IPlugin?)Activator.CreateInstance(type);
                            if (pluginInstance != null)
                            {
                                //  Find correct application name from PluginMappings
                                string? appName = _pluginMappings.FirstOrDefault(x => x.Value.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase)).Key;

                                if (appName != null)
                                { 
                                    _loadedPlugins[appName.ToLower()] = pluginInstance;
                                    _logger.LogInformation($" Plugin Loaded: {type.Name} for {appName}");
                                }
                                else
                                {
                                    _logger.LogWarning($"No mapping found for plugin {type.Name}, skipping registration.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to load plugin {file}: {ex.Message}");
                }
            }
            return _loadedPlugins;
        }

        public IPlugin? GetPluginForProcess(string processName)
        {
            _logger.LogInformation($"Checking Plugin for: {processName}");

            if (_loadedPlugins.TryGetValue(processName.ToLower(), out var plugin))
            {
                _logger.LogInformation($"Using Plugin: {plugin.GetType().Name} for {processName}");
                return plugin;
            }

            _logger.LogWarning($"No plugin loaded for: {processName}");
            return null;
        }
    }
}
