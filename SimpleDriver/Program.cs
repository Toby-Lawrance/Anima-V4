using System;
using System.IO;

namespace SimpleDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var anima = Anima.Core.Anima.CreateInstance(new BinaryWriter(Console.OpenStandardOutput()),
                new BinaryReader(Console.OpenStandardInput()), Console.Error);


        }
    }
}
