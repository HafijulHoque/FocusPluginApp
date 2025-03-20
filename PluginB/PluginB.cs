using PluginInterface;
namespace PluginB  // 👈 Add a unique namespace for PluginA
{
    public class PluginB : IPlugin
    {
        public void Execute()
        {
            Console.WriteLine("PluginB Executed for Firefox!");
        }
    }
}
