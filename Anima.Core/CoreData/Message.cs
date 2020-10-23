namespace Anima.Core
{
    public class Message
    {
        public string Sender;
        public string Receiver;

        public string Note;

        public object Value;

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
    }
}
