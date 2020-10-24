using Anima.Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Module = Anima.Core.Plugins.Module;

namespace Anima.Core.PluginManagement
{
    public class PluginManager
    {
        private string[] directories =
        {
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName + @"\Plugins"
        };

        private IEnumerable<Module> loadedPlugins;
        private IEnumerable<Timer> runningPlugins;


        public void LoadAndRunPlugins()
        {
            loadedPlugins = LoadPlugins();
            InitialisePlugins();
            runningPlugins = InitialisePluginTicks();
        }

        public void InitialisePlugins()
        {
            var initTasks = loadedPlugins.Select(plugin => Task.Run(plugin.Init));
            var succ = Task.WaitAll(initTasks.ToArray(),new TimeSpan(0,0,1,0));
            if (!succ)
            {
                Anima.Instance.ErrorStream.WriteLine("Plugin Initialization failed");
            }
        }


        public IEnumerable<Timer> InitialisePluginTicks()
        {
            return loadedPlugins.Select(plugin => new Timer(o => plugin.Tick(),null,plugin.TickDelay/2,plugin.TickDelay));
        }

        public void ClosePlugins()
        {
            var disposal = runningPlugins.Select(t => t.DisposeAsync());

            while (!disposal.All(vt => vt.IsCompleted))
            {
                Thread.Sleep(500);
            }
            
            var initTasks = loadedPlugins.Select(plugin => Task.Run(plugin.Close));
            var succ = Task.WaitAll(initTasks.ToArray(), new TimeSpan(0, 0, 2, 30));
            if (!succ)
            {
                Anima.Instance.ErrorStream.WriteLine("Plugin Closing failed");
            }
        }

        public IEnumerable<Module> LoadPlugins()
        {
            return directories.SelectMany(path =>
                new DirectoryInfo(path).EnumerateFiles().Where(fi => fi.Extension == ".dll")
                    .Select(fi => Assembly.LoadFile(fi.FullName))
                    .SelectMany(ass => ass.GetTypes())
                    .Where(t => typeof(Module).IsAssignableFrom(t))
                    .Select(t => Activator.CreateInstance(t) as Module)
            );
        }
    }
}
