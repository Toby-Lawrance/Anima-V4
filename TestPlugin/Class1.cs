using System;
using System.Linq;
using Anima.Core;
using Anima.Core.Plugins;

namespace TestPlugin
{
    public class TestModule : Module
    {
        private long Count = 0;
        private static string name = "Test Plugin";

        public TestModule() : base(name,"A test plugin",TimeSpan.FromSeconds(1)) {}

        public override void Tick()
        {
            Anima.Core.Anima.Instance.OutStream.WriteLine($"Count:{Count++}");
        }
    }


    public class TestModule2 : Module
    {
        public TestModule2() : base("Test Plugin 2", "Testing if multiple modules can be loaded from a single dll",TimeSpan.FromSeconds(3)) {}

        public override void Tick()
        {
            if (Anima.Core.Anima.Instance.MailBoxes.CheckNumMessages(this) == 0)
            {
                Anima.Core.Anima.Instance.OutStream.WriteLine($"No messages for: {this.Identifier}");
            }
            while (Anima.Core.Anima.Instance.MailBoxes.CheckNumMessages(this) > 0)
            {
                Anima.Core.Anima.Instance.OutStream.WriteLine(Anima.Core.Anima.Instance.MailBoxes.GetMessage(this));
            }
            
        }

        public class TestModule3 : Module
        {
            private int Count = 0;
            public TestModule3() : base("Embedded Plugin", "Test for embedded",TimeSpan.FromSeconds(1)) {}

            public override void Tick()
            {
                Count++;
                Anima.Core.Anima.Instance.MailBoxes.PostMessage(Message.CreateMessage(this, "Test Plugin 2", new String('a',Count)));
            }
        }
    }
}
