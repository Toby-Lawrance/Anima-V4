using System;
using System.IO;
using System.Net.Sockets;
using Anima.Core;
using Newtonsoft.Json;

namespace SimpleDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var anima = new Anima.Core.Anima();
            anima.Run();
        }
    }
}
