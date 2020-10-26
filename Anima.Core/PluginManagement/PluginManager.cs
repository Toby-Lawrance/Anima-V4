using Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Module = Core.Plugins.Module;

namespace Core.PluginManagement
{
    public class PluginManager
    {
        [JsonInclude]
        private string[] directories =
        {
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName,
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName + @"\Plugins"
        };

        private IEnumerable<Plugins.Module> loadedPlugins;
        private IEnumerable<Timer> runningPlugins;


        public bool AddPluginDirectory(DirectoryInfo d)
        {
            if (d.Exists)
            {
                directories = directories.Append(d.FullName).ToArray();
                return true;
            }

            return false;
        }

        public void LoadAndRunPlugins()
        {
            loadedPlugins = LoadPlugins();
            InitialisePlugins();
            runningPlugins = InitialisePluginTicks().ToList();
        }

        public void InitialisePlugins()
        {
            var initTasks = loadedPlugins.Select(plugin => Task.Run(plugin.Init));
            var succ = Task.WaitAll(initTasks.ToArray(),new TimeSpan(0,0,1,0));
            if (!succ)
            {
                Anima.Instance.ErrorStream.WriteLine("Plugin Initialization failed");
            }
            else
            {
                foreach (var plugin in loadedPlugins)
                {
                    Anima.Instance.WriteLine($"Initialized:{plugin}");
                }
            }
        }


        public IEnumerable<Timer> InitialisePluginTicks()
        {
            int startDelay = 1;
            return loadedPlugins.Select(plugin => new Timer(_ => plugin.Tick(),null,new TimeSpan(0,0,startDelay++), plugin.TickDelay));
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

        public IEnumerable<Plugins.Module> LoadPlugins()
        {
            return directories.Where(Directory.Exists).SelectMany(path =>
                new DirectoryInfo(path).EnumerateFiles().Where(fi => fi.Extension == ".dll")
                    .Select(fi => Assembly.LoadFile(fi.FullName))
                    .SelectMany(ass => ass.GetTypes())
                    .Where(t => typeof(Plugins.Module).IsAssignableFrom(t))
                    .Select(t => Activator.CreateInstance(t) as Plugins.Module)
            );
        }
    }
}
