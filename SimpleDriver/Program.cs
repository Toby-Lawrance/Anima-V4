using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using Core;

namespace SimpleDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Anima anima = null;
            var state = new FileInfo("AnimaState.json");
            if (state.Exists)
            {
                using (var fs = new StreamReader(state.OpenRead()))
                {
                    var contents = fs.ReadToEnd();
                    if (!String.IsNullOrWhiteSpace(contents))
                    {
                        anima = Anima.Deserialize<Anima>(contents);
                        if(anima is null) {Console.WriteLine("Loading failed");}
                        Console.WriteLine("Loaded in from state file");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Could not find: {state.FullName}");
            }
            anima ??= Anima.Instance;

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    //Save state
                    var newState = Anima.Serialize(anima);
                    //Quick way to remake file fresh
                    state.Delete();
                    using (var fs = new StreamWriter(state.Create()))
                    {
                        fs.Write(newState);
                    }
                };

            try
            {
                anima.Run();
            }
            finally
            {
                //Save state
                var newState = Anima.Serialize(anima);
                //Quick way to remake file fresh
                state.Delete();
                using (var fs = new StreamWriter(state.Create()))
                {
                    fs.Write(newState);
                }
            }
        }
    }
}
