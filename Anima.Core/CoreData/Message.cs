using Core.Plugins;
using Newtonsoft.Json;

namespace Core
{
    public class Message
    {
        public string Sender;
        public string Receiver;

        public string Note;

        public string Value;

        public Message() {}

        public Message(string sender, string receiver, string value)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            Value = value;
        }

        public Message(string sender, string receiver, string note, string value)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Note = note;
            Value = value;
        }

        public static Message CreateMessage(Module mod, string receiver, string val) => new Message(mod.Identifier,receiver,val);
        public static Message CreateMessageFromObject(Module mod, string receiver, object val) => new Message(mod.Identifier, receiver, Anima.Serialize(val));

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
