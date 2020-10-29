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

        public object WriteLine(object s)
        {
            OutStream.WriteLineAsync(s.ToString());
            return s;
        }

        public string WriteLine(string s)
        {
            OutStream.WriteLineAsync(s);
            return s;
        }

        public string ReadLine()
        {
            var t = InStream.ReadLineAsync();
            t.Wait();
            return t.Result ?? "";
        }

        public static readonly string NewLineChar = Environment.NewLine;
        public static readonly string EofToken = NewLineChar+"<EOF>"+NewLineChar;

        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj, Formatting.Indented,
            new JsonSerializerSettings() {Converters = { new MyTypedKeyValueConverter() } }) + EofToken;

        public static T Deserialize<T>(string json)
        {
            try
            {
                if (json.EndsWith(EofToken))
                {
                    json = json.Remove(json.Length - EofToken.Length);
                }
                return JsonConvert.DeserializeObject<T>(json,
                    new JsonSerializerSettings() { Converters = { new MyTypedKeyValueConverter() } });
            }
            catch (Exception e)
            {
                Anima.Instance.ErrorStream.WriteLine($"Error deserializing: {json}\n Due to: {e.Message}");
                return default(T);
            }
        }

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
