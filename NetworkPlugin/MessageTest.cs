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

                if (server is null)
                {
                    if (Anima.Instance.KnowledgePool.Exists("IP-Port"))
                    {
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Port", out int Port);
                        var info = Dns.GetHostEntry(Dns.GetHostName());
                        info.AddressList.Select(a => Anima.Instance.WriteLine($"Server Address: {a}"));
                        var address = info.AddressList[0];
                        var end = new IPEndPoint(address, Port);
                        server = new TcpListener(IPAddress.IPv6Any, Port);
                        server.Start();
                        SuccessfulSetup = true;
                    }
                    else
                    {
                        Anima.Instance.KnowledgePool.TryInsertValue("IP-Addresses", new string[] { });
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
                    //Anima.Instance.KnowledgePool.TrySetValue(kvp.Key, kvp.Value.Value);
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

        public class MessageClient : Core.Plugins.Module
        {
            private List<(TcpClient, StreamWriter)> outBoundClients;
            private int port;
            private bool SuccessfulSetup = false;
            private ManualResetEvent GetHostEntryFinished = new ManualResetEvent(false);

            public MessageClient() : base("MessageClient", "Sends messages to other computers", 10) { }

            private (bool, TcpClient) TryConnect(IPAddress a, int p, TcpClient tcp)
            {
                try
                {
                    tcp.Connect(a, p);
                    return (true, tcp);
                }
                catch (Exception e)
                {
                    Anima.Instance.ErrorStream.WriteLine($"Unable to connect to:{a} because {e.Message}");
                    return (false, tcp);
                }
            }

            private void TryAddresses(IAsyncResult state)
            {
                try
                {
                    var clientList = (List<TcpClient>)state.AsyncState;
                    var entry = Dns.EndGetHostEntry(state);

                    var connections = entry.AddressList.Select(a => (new TcpClient(AddressFamily.InterNetworkV6), a))
                        .Select(tcpa => Task.Run(() => TryConnect(tcpa.a, port, tcpa.Item1))).ToArray();
                    Task.WaitAll(connections);
                    var working = connections.Where(t => t.Result.Item1).Select(r => r.Result.Item2);
                    clientList.AddRange(working);
                    GetHostEntryFinished.Set();
                }
                catch (Exception e)
                {
                    Anima.Instance.ErrorStream.WriteLine(e.Message);
                    GetHostEntryFinished.Set();
                }
                
            }

            public override void Init()
            {
                base.Init();
                if (!IPv6Support)
                {
                    Anima.Instance.ErrorStream.WriteLine("IPv6 is not supported");
                    return;
                }

                if (outBoundClients is null)
                {
                    if (Anima.Instance.KnowledgePool.Exists("IP-Addresses") && Anima.Instance.KnowledgePool.Exists("IP-Port"))
                    {
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Addresses", out IEnumerable<string> addresses);
                        Anima.Instance.KnowledgePool.TryGetValue("IP-Port", out port);
                        outBoundClients = new List<(TcpClient, StreamWriter)>();
                        foreach (var s in addresses)
                        {
                            var clientList = new List<TcpClient>();
                            Anima.Instance.WriteLine($"Checking for hostname:{s}");
                            GetHostEntryFinished.Reset();
                            Dns.BeginGetHostEntry(s, TryAddresses, clientList);
                            GetHostEntryFinished.WaitOne();
                            if (clientList.Count > 0)
                            {
                                var workingClient = clientList.First();
                                if (workingClient.GetStream() != Stream.Null)
                                {
                                    outBoundClients.Add((workingClient, new StreamWriter(workingClient.GetStream())));
                                }
                                
                            }
                        }
                        SuccessfulSetup = true;
                    }
                    else
                    {
                        Anima.Instance.KnowledgePool.TryInsertValue("IP-Addresses", new string[] { });
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
                    string message = Anima.Serialize(new KeyValuePair<string, KeyValuePair<Type, object>>("Count", Anima.Instance.KnowledgePool.Pool["Count"]));
                    var tasks = outBoundClients.Select(tup => tup.Item2).Select(strem => strem.WriteLineAsync(message));
                    Task.WaitAll(tasks.ToArray());
                }
            }

            public override void Close()
            {
                base.Close();
                if (!SuccessfulSetup) return;
                foreach (var client in outBoundClients)
                {
                    client.Item1.Close();
                    client.Item2.Close();
                }
            }
        }
    }

}
