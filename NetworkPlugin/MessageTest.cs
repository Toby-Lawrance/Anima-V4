using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Core;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace NetworkPlugin
{
    public static class MessageHolder
    {
        private static bool IPv6Support = Socket.OSSupportsIPv6;

        public class MessageClient : Core.Plugins.Module
        {
            private List<(TcpClient,StreamWriter)> outBoundClients;
            private bool SuccessfulSetup = false;

            public MessageClient() : base("MessageClient", "Sends messages to other computers", 10) { }

            public override void Init()
            {
                base.Init();
                if(!IPv6Support)
                {
                    Anima.Instance.ErrorStream.WriteLine("IPv6 is not supported");
                    return;
                }

                if(outBoundClients is null) 
                { 
                    if(Anima.Instance.KnowledgePool.Exists("IP-Addresses") && Anima.Instance.KnowledgePool.Exists("IP-Port"))
                    {
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Addresses", out List<string> addresses);
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Port", out int Port);
                        outBoundClients = addresses.Select(s => new IPEndPoint(IPAddress.Parse(s), Port)).Select(end => new TcpClient(end)).Select(c => (c, new StreamWriter(c.GetStream()))).ToList();
                        SuccessfulSetup = true;
                    } else
                    {
                        Anima.Instance.KnowledgePool.TryInsertValue("IP-Addresses", new List<string>());
                        Anima.Instance.KnowledgePool.TryInsertValue("IP-Port", 0);
                        Anima.Instance.ErrorStream.WriteLine("Error: Needed to create values in Anima pool");
                        outBoundClients = new List<(TcpClient, StreamWriter)>();
                    }
                }
            }

            public override void Tick()
            {
                if (!SuccessfulSetup) return;
                if (Anima.Instance.KnowledgePool.Exists("Count"))
                {
                    string message = Anima.Serialize(new KeyValuePair<string,KeyValuePair<Type,object>>("Count",Anima.Instance.KnowledgePool.Pool["Count"]));
                    var tasks = outBoundClients.Select(tup => tup.Item2).Select(strem => strem.WriteLineAsync(message));
                    Task.WaitAll(tasks.ToArray());
                }
            }

            public override void Close()
            {
                base.Close();
                if (!SuccessfulSetup) return;
                foreach(var client in outBoundClients)
                {
                    client.Item1.Close();
                    client.Item2.Close();
                }
            }
        }

        public class MessageServer : Core.Plugins.Module
        {
            private StreamReader inStream;
            private TcpListener server;
            private bool SuccessfulSetup = false;

            public MessageServer() : base("MessageServer", "Receives messages from other servers", 1) { }

            public override void Init()
            {
                base.Init();
                if (!IPv6Support)
                {
                    Anima.Instance.ErrorStream.WriteLine("IPv6 is not supported");
                    return;
                }

                if(server is null)
                {
                    if (Anima.Instance.KnowledgePool.Exists("IP-Addresses") && Anima.Instance.KnowledgePool.Exists("IP-Port"))
                    {
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Addresses", out List<string> addresses);
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Port", out int Port);
                        var info = Dns.GetHostEntry(Dns.GetHostName());
                        info.AddressList.Select(a => Anima.Instance.WriteLine(a));
                        var address = info.AddressList[0];
                        var end = new IPEndPoint(address, Port);
                        server = new TcpListener(end);
                        server.Start();
                        SuccessfulSetup = true;
                    } else
                    {
                        Anima.Instance.KnowledgePool.TryInsertValue("IP-Addresses", new List<string>());
                        Anima.Instance.KnowledgePool.TryInsertValue("IP-Port", 0);
                        Anima.Instance.ErrorStream.WriteLine("Error: Needed to create values in Anima pool");
                    }
                }
            }

            public override void Tick()
            {
                if (!SuccessfulSetup) return;
                try
                {
                    TcpClient client = server.AcceptTcpClient();

                    inStream = new StreamReader(client.GetStream());

                    var s = inStream.ReadLine();
                    Anima.Instance.WriteLine($"Received: {s} from: {client.Client.RemoteEndPoint}");
                    var kvp = Anima.Deserialize<KeyValuePair<string, KeyValuePair<Type, object>>>(s);
                    Anima.Instance.KnowledgePool.TrySetValue(kvp.Key, kvp.Value.Value);
                }
                catch (Exception e)
                {
                    Anima.Instance.ErrorStream.WriteLine(e);
                }
            }

            public override void Close()
            {
                base.Close();
                if (!SuccessfulSetup) return;
                server.Stop();
            }
        }
    }
    
}
