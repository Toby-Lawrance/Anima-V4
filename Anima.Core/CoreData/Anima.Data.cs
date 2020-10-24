using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Anima.Core.CoreData;

namespace Anima.Core
{
    public partial class Anima {

        private MailSystem mailBoxes;
        private KnowledgeBase pool;

        public MailSystem MailBoxes => mailBoxes;
        public KnowledgeBase KnowledePool => pool;
    }
}