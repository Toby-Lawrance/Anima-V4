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
                        this.StartTask(ListenAndRespond,TaskCreationOptions.LongRunning);
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

            private void ListenAndRespond()
            {
                while (true)
                {
                    try
                    {
                        var client = server.AcceptTcpClient();

                        inStream = new StreamReader(client.GetStream());

                        string ReadContents = "";
                        string line = "";
                        while ((line = inStream.ReadLine()) != "<EOF>")
                        {
                            ReadContents += line + Anima.NewLineChar;
                        }
                        Anima.Instance.WriteLine($"Received: {ReadContents} from: {client.Client.RemoteEndPoint}");
                        Anima.Instance.MailBoxes.PostMessage(new Message(client.Client.RemoteEndPoint.ToString(), this.Identifier,
                            "Remote", ReadContents));
                        client.Close();
                    }
                    catch (Exception e)
                    {
                        Anima.Instance.ErrorStream.WriteLine(e);
                    }
                }
            }

            public override void Tick()
            {
                if (!SuccessfulSetup) return;
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

            public MessageClient() : base("MessageClient", "Sends messages to other computers", 2) { }

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
                        foreach (var sAddress in addresses)
                        {
                            Anima.Instance.WriteLine($"Checking for hostname:{sAddress} as an IP address");
                            var IP = IPAddress.Parse(sAddress);
                            var tcp = new TcpClient(AddressFamily.InterNetworkV6);
                            var t = Task.Run(() => TryConnect(IP, port, tcp));
                            t.Wait();
                            if (t.Result.Item1)
                            {
                                outBoundClients.Add((t.Result.Item2,new StreamWriter(t.Result.Item2.GetStream())));
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
                //if (!SuccessfulSetup) return;
                if (Anima.Instance.KnowledgePool.Exists("Count"))
                {
                    Anima.Instance.WriteLine($"Attempting to outWrite");
                    string message = Anima.Serialize(new KeyValuePair<string, KeyValuePair<Type, object>>("Count", Anima.Instance.KnowledgePool.Pool["Count"]));
                    var tasks = outBoundClients.Select(tup => tup.Item2).Select(strem => Task.Run(() =>
                    {
                        strem.WriteLine(message);
                        strem.Flush();
                    })).ToArray();
                    Task.WaitAll(tasks);
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
