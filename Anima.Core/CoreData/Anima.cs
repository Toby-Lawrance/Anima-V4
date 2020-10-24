using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using Anima.Core.CoreData;
using Newtonsoft.Json;

namespace Anima.Core
{
    public partial class Anima
    {
        private static Anima _instance;
        public static Anima Instance => _instance ??= new Anima();


        public Anima(BinaryWriter ostream, BinaryReader istream, TextWriter estream)
        {
            SetStreams(ostream,istream,estream);
            pool = new KnowledgeBase();
            mailBoxes = new MailSystem();

            _instance ??= this;
        }

        public Anima() : this(null, null, null) { }

        public void SetStreams(BinaryWriter ostream, BinaryReader istream, TextWriter estream)
        {
            outStream = ostream ?? new BinaryWriter(Console.OpenStandardOutput());
            inStream = istream ?? new BinaryReader(Console.OpenStandardInput());
            errorStream = estream ?? Console.Error;
        }

        [JsonIgnore]
        public BinaryWriter OutStream => outStream;
        [JsonIgnore]
        public BinaryReader InStream => inStream;
        [JsonIgnore]
        public TextWriter ErrorStream => errorStream;

        [JsonIgnore]
        private BinaryWriter outStream;
        [JsonIgnore]
        private BinaryReader inStream;
        [JsonIgnore]
        private TextWriter errorStream;
    }
}
