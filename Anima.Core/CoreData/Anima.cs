using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Anima.Core.CoreData;
using Anima.Core.PluginManagement;
using Newtonsoft.Json;

namespace Anima.Core
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
