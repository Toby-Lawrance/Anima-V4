using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.String;


namespace Anima.Core.CoreData
{
    public class MailSystem
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message>> _mailBoxes;

        [JsonIgnore]
        public ConcurrentDictionary<string, ConcurrentQueue<Message>> MailBoxes => _mailBoxes;

        public MailSystem()
        {
            _mailBoxes = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
        }


        public bool PostMessage(Message value)
        {
            if (value is null)
            {
                return false;
            }

            string receiver = value.Receiver;
            if (IsNullOrWhiteSpace(receiver))
            {
                return false;
            }

            if (!_mailBoxes.ContainsKey(receiver))
            {
                _mailBoxes[receiver] = new ConcurrentQueue<Message>();
            }

            _mailBoxes[receiver].Enqueue(value);
            return true;
        }

        public int CheckNumMessages(string id)
        {
            if (!_mailBoxes.ContainsKey(id))
            {
                return 0;
            }

            return _mailBoxes[id].Count;
        }

        public Message GetMessage(string id)
        {
            if (!_mailBoxes.ContainsKey(id))
            {
                return null;
            }

            if (CheckNumMessages(id) > 0)
            {
                var result = _mailBoxes[id].TryDequeue(out Message firstMessage);
                if (result)
                {
                    return firstMessage;
                }
            }

            //No messages or unable to dequeue
            return null;
        }
    }
}
