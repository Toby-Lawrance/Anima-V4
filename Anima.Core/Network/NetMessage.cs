using System;
using System.Text.Json.Serialization;

namespace Core.Network
{
    public class NetMessage
    {
        [JsonInclude]
        public string SendHost = "";
        [JsonInclude]
        public string ReceiveHost = "";

        [JsonInclude]
        public string SendPlugin = "";
        [JsonInclude]
        public string ReceivePlugin = "";

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
            SendPlugin = sendPlugin;
            ReceivePlugin = receivePlugin;
            Value = value;
        }
    }
}