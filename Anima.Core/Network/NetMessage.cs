using System;
using System.Text.Json.Serialization;
using Core.CoreData;

namespace Core.Network
{
    public class NetMessage : MessageBase
    {
        [JsonInclude]
        public string SendHost = "";
        [JsonInclude]
        public string ReceiveHost = "";

        [JsonInclude]
        public bool GetRequest = false;

        [JsonInclude]
        public string Value = "";

        public NetMessage() {}

        public NetMessage(bool Get)
        {
            if (Get)
            {
                SendHost = Environment.MachineName;
                GetRequest = true;
            }
        }

        public NetMessage(string receiveHost, string sendPlugin, string receivePlugin, string value)
        {
            SendHost = Environment.MachineName;
            ReceiveHost = receiveHost;
            Sender = sendPlugin;
            Receiver = receivePlugin;
            Value = value;
        }
    }
}