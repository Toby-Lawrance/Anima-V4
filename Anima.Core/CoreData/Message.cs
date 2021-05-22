using Core.CoreData;
using Core.Plugins;
using Newtonsoft.Json;

namespace Core
{
    public class Message<T> : MessageBase
    {
        public T Value;

        public Message(string sender, string receiver, T value, string note = "", string receiveBox = "default", string sendReturnBox = "default")
        {
            this.Sender = sender;
            this.Receiver = receiver;
            Value = value;
        }
        
        public static Message<T> CreateMessage(Module mod, string receiver, T val) => new(mod.Identifier,receiver,val);

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
