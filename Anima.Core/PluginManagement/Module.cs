using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Plugins
{
    public abstract class Module
    {
        public readonly string Identifier;
        public readonly string Description;
        public readonly TimeSpan TickDelay;

        protected Module(string id,string desc, TimeSpan delay)
        {
            Identifier = id;
            Description = desc;
            TickDelay = delay;
        }
        protected Module(string id, string desc = "", int tick = 1) : this(id,desc,new TimeSpan(0,0,0,tick)) {}

        public virtual void Init() {}
        public abstract void Tick();
        public virtual void Close() {}

        public override string ToString() => $"{Identifier}:\n\t{Description}\n\tFires every {TickDelay}";
    }
}
