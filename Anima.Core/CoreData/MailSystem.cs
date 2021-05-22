using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Plugins;
using static System.String;


namespace Core.CoreData
{
    public class MailSystem
    {
        [JsonInclude]
        public ConcurrentDictionary<string, MailBox> MailBoxes { get; }

        public MailSystem()
        {
            MailBoxes = new ConcurrentDictionary<string, MailBox>();
        }
        
        public bool PostMessage<T>(Message<T>? value)
        {
            if (value is null)
            {
                return false;
            }

            var receiver = value.Receiver;
            if (IsNullOrWhiteSpace(receiver))
            {
                return false;
            }

            if (!MailBoxes.ContainsKey(receiver))
            {
                MailBoxes[receiver] = new MailBox();
            }

            MailBoxes[receiver].PostMessage(value);
            return true;
        }

        public int CheckNumMessages(Module mod)
        {
            var id = mod.Identifier;
            return !MailBoxes.ContainsKey(id) ? 0 : MailBoxes[id].CheckTotalNumMessages();
        }

        public Message<T>? GetMessage<T>(Module mod, string box = "default")
        {
            var id = mod.Identifier;
            return !MailBoxes.ContainsKey(id) ? null : MailBoxes[id].GetMessage<T>(box);
        }
    }
}
