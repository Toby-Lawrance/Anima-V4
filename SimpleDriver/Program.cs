using System;
using System.IO;
using Anima.Core;
using Newtonsoft.Json;

namespace SimpleDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var anima = new Anima.Core.Anima(new BinaryWriter(Console.OpenStandardOutput()),
                new BinaryReader(Console.OpenStandardInput()), Console.Error);


            anima.MailBoxes.PostMessage(new Message("1","2","cheese"));
            anima.MailBoxes.PostMessage(new Message("1", "3", "cheese2"));
            anima.MailBoxes.PostMessage(new Message("1", "4", "cheese3"));
            anima.MailBoxes.PostMessage(new Message("1", "2", "cheese4"));

            string output = JsonConvert.SerializeObject(anima);

            Console.WriteLine(output);


            Anima.Core.Anima deserializedAnima = JsonConvert.DeserializeObject<Anima.Core.Anima>(output);
            deserializedAnima.SetStreams(new BinaryWriter(Console.OpenStandardOutput()),
                new BinaryReader(Console.OpenStandardInput()), Console.Error);
            var myMessage = deserializedAnima.MailBoxes.GetMessage("3");
        }
    }
}
