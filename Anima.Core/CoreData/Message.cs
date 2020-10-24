using Anima.Core.Plugins;
using Newtonsoft.Json;

namespace Anima.Core
{
    public class Message
    {
        public string Sender;
        public string Receiver;

        public string Note;

        public object Value;

        public Message() {}

        public Message(string sender, string receiver, object value)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            Value = value;
        }

        public Message(string sender, string receiver, string note, object value)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Note = note;
            Value = value;
        }

        public static Message CreateMessage(Module mod, string receiver, object val) => new Message(mod.Identifier,receiver,val);

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
