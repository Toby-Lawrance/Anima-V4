using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Core.CoreData;
using Core.PluginManagement;
using Newtonsoft.Json;

namespace Core
{
    public partial class Anima
    {
        private static Anima _instance;
        public static Anima Instance => _instance ??= new Anima();


        public Anima(TextWriter ostream, TextReader istream, TextWriter estream)
        {
            SetStreams(ostream,istream,estream);
            pool = new KnowledgeBase();
            mailBoxes = new MailSystem();
            plugMan = new PluginManager();

            _instance ??= this;
        }

        public Anima() : this(null, null, null) { }

        public void SetStreams(TextWriter ostream, TextReader istream, TextWriter estream)
        {
            outStream = ostream ?? Console.Out;
            inStream = istream ?? Console.In;
            errorStream = estream ?? Console.Error;
        }

        public void Run()
        {
            OutStream.WriteLine("Initializing Anima");
            plugMan.LoadAndRunPlugins();
            OutStream.WriteLine("Plugin manager complete");
            string input;
            do
            {
                input = InStream.ReadLine();
            } while (input != "quit");
            plugMan.ClosePlugins();
        }

        public void WriteLine(object s)
        {
            OutStream.WriteLineAsync(s.ToString());
        }

        public string ReadLine()
        {
            var t = InStream.ReadLineAsync();
            t.Wait();
            return t.Result ?? "";
        }

        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj, Formatting.Indented,
            new JsonSerializerSettings() {Converters = { new MyTypedKeyValueConverter() } });

        public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json,
            new JsonSerializerSettings() {Converters = { new MyTypedKeyValueConverter() } });

        [JsonIgnore]
        public TextWriter OutStream => outStream;
        [JsonIgnore]
        public TextReader InStream => inStream;
        [JsonIgnore]
        public TextWriter ErrorStream => errorStream;

        [JsonIgnore]
        private TextWriter outStream;
        [JsonIgnore]
        private TextReader inStream;
        [JsonIgnore]
        private TextWriter errorStream;
    }
}
