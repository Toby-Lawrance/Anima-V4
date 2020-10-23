using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;

namespace Anima.Core
{
    public partial class Anima : ISerializable
    {
        private ConcurrentDictionary<string, ISerializable> KnowledgePool;

        private ConcurrentDictionary<string, ConcurrentQueue<Message>> MailBoxes;

        public bool postMessage(Message value)
        {
            if (MailBoxes is null)
            {
                MailBoxes = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
            }

            if (value is null)
            {
                return false;
            }

            string receiver = value.receiver;
            if (String.IsNullOrWhiteSpace(receiver))
            {
                return false;
            }

            if (MailBoxes[receiver] is null)
            {
                MailBoxes[receiver] = new ConcurrentQueue<Message>();
            }

            MailBoxes[receiver].Enqueue(value);
            return true;
        }

        public int checkNumMessages(string id)
        {
            if (MailBoxes is null)
            {
                MailBoxes = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
            }

            if (MailBoxes[id] is null)
            {
                return 0;
            }

            return MailBoxes[id].Count;
        }

        public Message getMessage(string id)
        {
            if (MailBoxes is null)
            {
                MailBoxes = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
            }

            if (MailBoxes[id] is null)
            {
                return null;
            }

            if (checkNumMessages(id) > 0)
            {
                Message firstMessage;
                var result = MailBoxes[id].TryDequeue(out firstMessage);
                if (result)
                {
                    return firstMessage;
                }
            }

            //No messages or unable to dequeue
            return null;
        }

        private Anima(SerializationInfo info, StreamingContext context)
        {
            var deserializedPool =
                (KeyValuePair<string, ISerializable>[]) info.GetValue("pool",
                    typeof(KeyValuePair<string, ISerializable>[]));
            KnowledgePool = deserializedPool is null ? new ConcurrentDictionary<string, ISerializable>() : new ConcurrentDictionary<string, ISerializable>(deserializedPool);

            var deserializedMessageQueues =
                (KeyValuePair<string, Message[]>[]) info.GetValue("messageQueues",
                    typeof(KeyValuePair<string, Message[]>[]));
            if (deserializedMessageQueues is null)
            {
                MailBoxes = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
            }
            else
            {
                var inForm = deserializedMessageQueues.Select(kvp =>
                    new KeyValuePair<string, ConcurrentQueue<Message>>(kvp.Key,
                        new ConcurrentQueue<Message>(kvp.Value)));
                MailBoxes = new ConcurrentDictionary<string, ConcurrentQueue<Message>>(inForm);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("pool", KnowledgePool.ToArray(), typeof(KeyValuePair<string, ISerializable>[]));

            var mailBoxArr = MailBoxes.ToArray().Select(kvp => new KeyValuePair<string,Message[]>(kvp.Key,kvp.Value.ToArray())).ToArray();
            info.AddValue("messageQueues", mailBoxArr, typeof(KeyValuePair<string,Message[]>[]));
        }
    }
}