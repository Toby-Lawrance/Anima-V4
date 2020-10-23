using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;

namespace Anima.Core
{
    [Serializable]
    public partial class Anima
    {
        //Singleton instance
        [NonSerialized] private static Anima instance;

        private Anima(BinaryWriter ostream, BinaryReader istream, TextWriter estream)
        {
            outStream = ostream ?? new BinaryWriter(Console.OpenStandardOutput());
            inStream = istream ?? new BinaryReader(Console.OpenStandardInput());
            errorStream = estream ?? Console.Error;
        }

        public static Anima Instance
        {
            get
            {
                if (instance is null)
                {
                    throw new Exception("Instance not created");
                }

                return instance;
            }
        }

        public static Anima CreateInstance(BinaryWriter ostream, BinaryReader istream, TextWriter estream, SerializationInfo info = null)
        {
            if (instance is not null)
            {
                throw new Exception("Instance already created");
            }

            if (info is null)
            {
                instance = new Anima(ostream, istream, estream);
                return instance;
            }

            instance = new Anima(info,new StreamingContext());
            instance.outStream = ostream;
            instance.inStream = istream;
            instance.errorStream = estream;
            return instance;
        }


        public BinaryWriter OutStream => outStream;
        public BinaryReader InStream => inStream;
        public TextWriter ErrorStream => errorStream;

        [NonSerialized]
        private BinaryWriter outStream;
        [NonSerialized]
        private BinaryReader inStream;
        [NonSerialized]
        private TextWriter errorStream;
    }
}
