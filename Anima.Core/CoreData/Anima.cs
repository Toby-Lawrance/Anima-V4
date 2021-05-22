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


        public Anima(TextWriter? ostream, TextReader? istream, TextWriter? estream)
        {
            SetStreams(ostream, istream, estream);
            SystemMail = new MailSystem();
            plugMan = new PluginManager();

            _instance ??= this;
        }

        public Anima() : this(null, null, null)
        {
        }

        public void SetStreams(TextWriter? ostream, TextReader? istream, TextWriter? estream)
        {
            OutStream = ostream ?? Console.Out;
            InStream = istream ?? Console.In;
            ErrorStream = estream ?? Console.Error;
        }

        public void Run()
        {
            OutStream.WriteLine("Initializing Anima");
            plugMan.LoadAndRunPlugins();
            OutStream.WriteLine("Plugin manager complete");
            string? input;
            do
            {
                input = InStream.ReadLine();
            } while (input != "quit");

            plugMan.ClosePlugins();
        }

        public object WriteLine(object s)
        {
            if (OutStream is null)
            {
                throw new Exception("No Output Stream assigned");
            }

            OutStream.WriteLineAsync(s.ToString());
            return s;
        }

        public string WriteLine(string s)
        {
            if (OutStream is null)
            {
                throw new Exception("No Output Stream assigned");
            }

            OutStream.WriteLineAsync(s);
            return s;
        }

        public string ReadLine()
        {
            if (InStream is null)
            {
                throw new Exception("No Input Stream assigned");
            }

            var t = InStream.ReadLineAsync();
            t.Wait();
            return t.Result ?? "";
        }

        public static readonly string NewLineChar = Environment.NewLine;
        public static readonly string EofToken = NewLineChar + "<EOF>" + NewLineChar;

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented) + EofToken;
        }

        public static T? Deserialize<T>(string json)
        {
            try
            {
                if (json.EndsWith(EofToken))
                {
                    json = json.Remove(json.Length - EofToken.Length);
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Instance.ErrorStream.WriteLine($"Error deserializing: {json}\n Due to: {e.Message}");
                return default;
            }
        }

        [JsonIgnore] public TextWriter OutStream { get; private set; }

        [JsonIgnore] public TextReader InStream { get; private set; }

        [JsonIgnore] public TextWriter ErrorStream { get; private set; }
    }
}