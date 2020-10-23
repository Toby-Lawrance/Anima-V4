using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Anima.Core
{
    [Serializable()]
    public class Message
    {
        public string sender;
        public string receiver;

        public string note;

        public ISerializable value;
    }
}
