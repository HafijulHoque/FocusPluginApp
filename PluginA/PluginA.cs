using PluginInterface;
namespace PluginA  // 👈 Add a unique namespace for PluginA
{
    public class PluginA : IPlugin
    {
        public void Execute()
        {
            Console.WriteLine("PluginA Executed for Notepad!");
        }
    }
}
