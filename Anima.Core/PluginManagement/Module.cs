using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Plugins
{
    public abstract class Module
    {
        public readonly string Identifier;
        public readonly string Description;
        public readonly TimeSpan TickDelay;
        public readonly bool Enabled;
        public readonly CancellationTokenSource Cancellation;
        protected List<Task> RunningTasks;

        protected Module(string id,string desc, TimeSpan delay,bool enabled = true)
        {
            Identifier = id;
            Description = desc;
            TickDelay = delay;
            Enabled = enabled;
            Cancellation = new CancellationTokenSource();
            RunningTasks = new List<Task>();
        }
        protected Module(string id, string desc = "", int tick = 1, bool enabled = true) : this(id,desc,new TimeSpan(0,0,0,tick),enabled) {}

        public virtual void Init() {}
        public abstract void Tick();
        public virtual void Close() { Cancellation.Cancel(); }

        public void StartTask(Action a,TaskCreationOptions tco = TaskCreationOptions.None)
        {
            TaskFactory tf = new TaskFactory(Cancellation.Token, tco, TaskContinuationOptions.None, null);
            var task = tf.StartNew(a);
            RunningTasks.Add(task);
        }

        public override string ToString() => $"{Identifier}:\n\t{Description}\n\tFires every {TickDelay}";
    }
}
