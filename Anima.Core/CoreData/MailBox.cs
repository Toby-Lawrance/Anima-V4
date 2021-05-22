using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json.Serialization;
using Core.Plugins;
using System.Text;

namespace Core.CoreData
{
    public class MailBox
    {
        [JsonInclude]
        public ConcurrentDictionary<string, ConcurrentQueue<MessageBase>> MailBoxes { get; }

        public bool PostMessage(MessageBase value)
        {
            if (value is null)
            {
                return false;
            }

            string receiver = value.ReceiverBox;
            if (string.IsNullOrWhiteSpace(receiver))
            {
                receiver = "default";
            }

            if (!MailBoxes.ContainsKey(receiver))
            {
                MailBoxes[receiver] = new ConcurrentQueue<MessageBase>();
            }

            MailBoxes[receiver].Enqueue(value);
            return true;
        }

        public int CheckNumMessages(string box = "default")
        {
            return !MailBoxes.ContainsKey(box) ? 0 : MailBoxes[box].Count;
        }

        public int CheckTotalNumMessages()
        {
            return MailBoxes.Select(kvp => kvp.Value).Aggregate(0,(sum,queue) => sum += queue.Count);
        }

        public Message<T> GetMessage<T>(string box = "default")
        {
            if (!MailBoxes.ContainsKey(box))
            {
                return null;
            }

            if (CheckNumMessages(box) <= 0)
            {
                return null;
            }

            var result = MailBoxes[box].TryPeek(out MessageBase firstMessage);
            if (!result || firstMessage is not Message<T> m)
            {
                return null;
            }

            MailBoxes[box].TryDequeue(out var first);
            return m;
        }
    }
}