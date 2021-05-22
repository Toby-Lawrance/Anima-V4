using Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Core.CoreData;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using Module = Core.Plugins.Module;

namespace Core.PluginManagement
{
    public class PluginManager
    {
        [JsonInclude] public string[] directories =
        {
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName
        };

        [JsonInclude] public KnowledgeBase<string[]> pluginInfo = new KnowledgeBase<string[]>();

        [Newtonsoft.Json.JsonIgnore]
        public List<Plugins.Module> loadedPlugins;
        [Newtonsoft.Json.JsonIgnore]
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

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
        }

        public List<Plugins.Module> LoadPlugins()
        {
            if (!pluginInfo.Exists("Enabled-Plugins"))
            {
                pluginInfo.TryInsertValue("Enabled-Plugins", new string[] { });
            }

            if (!pluginInfo.Exists("Disabled-Plugins"))
            {
                pluginInfo.TryInsertValue("Disabled-Plugins", new string[] { });
            }
            
            pluginInfo.TryGetValue("Enabled-Plugins", out string[] IEenabled);
            pluginInfo.TryGetValue("Disabled-Plugins", out string[] IEdisabled);
            var enabled = IEenabled.ToList();
            var disabled = IEdisabled.ToList();

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            var mods = directories.Where(Directory.Exists).SelectMany(path =>
                new DirectoryInfo(path).EnumerateFiles().Where(fi => fi.Extension == ".dll")
                    .Select(fi => Assembly.LoadFile(fi.FullName))
                    .Select(ass =>
                    {
                        foreach (var assembly in ass.GetReferencedAssemblies())
                        {
                            Assembly.Load(assembly);
                        }
                        return ass;
                    })
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
                        else if (!inEnabled && !inDisabled && !m.Enabled)
                        {
                            disabled.Add(m.Identifier);
                            Anima.Instance.WriteLine($"Adding unknown plugin:{m.Identifier} to the Disabled list");
                        }
                        else if (inDisabled)
                        {
                            m.Enabled = false;
                        }

                        return m;
                    })
                    .Where(m => m.Enabled)
            ).ToList();

            pluginInfo.TrySetValue("Enabled-Plugins", enabled.ToArray());
            pluginInfo.TrySetValue("Disabled-Plugins",disabled.ToArray());
            return mods;
        }
    }

    public class PlugManagerSerializationConverter : Newtonsoft.Json.JsonConverter<PluginManager>
    {
        public override void WriteJson(JsonWriter writer, PluginManager value, JsonSerializer serializer)
        {
            JObject o = JObject.FromObject(value);

            Dictionary<string, string> pluginSerialisations = new Dictionary<string, string>();
            foreach (var plugin in value.loadedPlugins)
            {
                pluginSerialisations[plugin.Identifier] = plugin.Serialize();
            }

            var obj = JObject.FromObject(pluginSerialisations, serializer);
            o.Add("plugin-data", obj);
            
            o.WriteTo(writer);
        }

        public override PluginManager ReadJson(JsonReader reader, Type objectType, PluginManager existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (hasExistingValue)
            {
                Anima.Instance.WriteLine("Filling out plugman");
                return existingValue;
            }
            else
            {
                Anima.Instance.WriteLine("Making new plugman");
                return new PluginManager();
            }
        }
    }
}