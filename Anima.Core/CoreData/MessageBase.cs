namespace Core.CoreData
{
    public abstract class MessageBase
    {
        public string Sender;
        public string SenderReturnBox = "default";
        public string Receiver;
        public string ReceiverBox = "default";

        public string Note;
    }
}