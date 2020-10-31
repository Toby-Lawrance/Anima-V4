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
        [JsonInclude] private string[] directories =
        {
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName,
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName + @"\Plugins"
        };

        private List<Plugins.Module> loadedPlugins;
        private List<Timer> runningPlugins;


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
            foreach (var directory in directories)
            {
                var d = new DirectoryInfo(directory);
                if (!d.Exists)
                {
                    d.Create();
                }
            }

            loadedPlugins = LoadPlugins();
            InitialisePlugins();
            runningPlugins = InitialisePluginTicks();
        }

        public void InitialisePlugins()
        {
            var initTasks = loadedPlugins.Select(plugin => Task.Run(() =>
            {
                plugin.Init();
                Anima.Instance.WriteLine($"Initialized:{plugin}");
            }));
            var succ = Task.WaitAll(initTasks.ToArray(), new TimeSpan(0, 0, 1, 0));
            if (!succ)
            {
                Anima.Instance.ErrorStream.WriteLine("Plugin Initialization failed");
            }
        }


        public List<Timer> InitialisePluginTicks()
        {
            int startDelay = 1;
            return loadedPlugins.Select(plugin =>
                new Timer(_ => plugin.Tick(), null, new TimeSpan(0, 0, startDelay++), plugin.TickDelay)).ToList();
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

        public List<Plugins.Module> LoadPlugins()
        {
            if (!Anima.Instance.KnowledgePool.Exists("Enabled-Plugins"))
            {
                Anima.Instance.KnowledgePool.TryInsertValue("Enabled-Plugins", new string[] { });
            }

            if (!Anima.Instance.KnowledgePool.Exists("Disabled-Plugins"))
            {
                Anima.Instance.KnowledgePool.TryInsertValue("Disabled-Plugins", new string[] { });
            }

            Anima.Instance.KnowledgePool.TryGetValue("Enabled-Plugins", out IEnumerable<string> IEenabled);
            Anima.Instance.KnowledgePool.TryGetValue("Disabled-Plugins", out IEnumerable<string> IEdisabled);
            var enabled = IEenabled.ToList();
            var disabled = IEdisabled.ToList();

            var mods = directories.Where(Directory.Exists).SelectMany(path =>
                new DirectoryInfo(path).EnumerateFiles().Where(fi => fi.Extension == ".dll")
                    .Select(fi => Assembly.LoadFile(fi.FullName))
                    .SelectMany(ass => ass.GetReferencedAssemblies().Select(AppDomain.CurrentDomain.Load).Append(ass))
                    .SelectMany(ass => ass.GetTypes())
                    .Where(t => typeof(Plugins.Module).IsAssignableFrom(t))
                    .Select(t => Activator.CreateInstance(t) as Plugins.Module)
                    .Select(m =>
                    {
                        var inEnabled = enabled.Contains(m.Identifier);
                        var inDisabled = disabled.Contains(m.Identifier);
                        if (!inEnabled && !inDisabled && m.Enabled)
                        {
                            enabled.Add(m.Identifier);
                            Anima.Instance.WriteLine($"Adding unknown plugin:{m.Identifier} to the Enabled list");
                        }
                        else if (inDisabled)
                        {
                            m.Enabled = false;
                        }

                        return m;
                    })
                    .Where(m => m.Enabled)
            ).ToList();
            Anima.Instance.WriteLine($"Enabled Plugin count: {enabled.Count}");
            Anima.Instance.WriteLine($"Disabled Plugin count: {disabled.Count}");

            Anima.Instance.KnowledgePool.TrySetValue("Enabled-Plugins", enabled.ToArray());
            Anima.Instance.KnowledgePool.TrySetValue("Disabled-Plugins",disabled.ToArray());
            return mods;
        }
    }
}