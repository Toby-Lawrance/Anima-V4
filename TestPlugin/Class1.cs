using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Plugins;

namespace TestPlugin
{
    public class TestModule : Module
    {
        private static string name = "Test Plugin";

        public TestModule() : base(name,"A test plugin",TimeSpan.FromSeconds(1)) {}

        public override void Init()
        {
            base.Init();
            Anima.Instance.KnowledgePool.TryInsertValue("Count", 0);
        }

        public override void Tick()
        {
            var succ = Core.Anima.Instance.KnowledgePool.TryGetValue("Count",out int Count);
            if (succ)
            {
                Anima.Instance.OutStream.WriteLine($"Count:{Count}");
                Anima.Instance.KnowledgePool.SetValue("Count", Count + 1);
            }
            else
            {
                Anima.Instance.ErrorStream.WriteLine("Couldn't get Count");
            }
        }
    }


    public class TestModule2 : Module
    {
        public TestModule2() : base("Test Plugin 2", "Testing if multiple modules can be loaded from a single dll",TimeSpan.FromSeconds(3)) {}

        public override void Tick()
        {
            if (Core.Anima.Instance.MailBoxes.CheckNumMessages(this) == 0)
            {
                Core.Anima.Instance.OutStream.WriteLine($"No messages for: {this.Identifier}");
            }
            while (Core.Anima.Instance.MailBoxes.CheckNumMessages(this) > 0)
            {
                Core.Anima.Instance.OutStream.WriteLine(Core.Anima.Instance.MailBoxes.GetMessage(this));
            }
            
        }

        public class TestModule3 : Module
        {
            public TestModule3() : base("Embedded Plugin", "Test for embedded",TimeSpan.FromSeconds(1)) {}

            public override void Init()
            {
                base.Init();
                Anima.Instance.KnowledgePool.TryInsertValue("Count", 0);
            }

            public override void Tick()
            {
                var succ = Anima.Instance.KnowledgePool.TryGetValue("Count", out int Count);
                Anima.Instance.MailBoxes.PostMessage(Message.CreateMessage(this, "Test Plugin 2", string.Concat(Enumerable.Repeat("a",Count))));
            }
        }
    }
}
