using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Core.CoreData;
using Core.PluginManagement;
using Newtonsoft.Json;

namespace Core
{
    public partial class Anima {

        private MailSystem mailBoxes;
        private KnowledgeBase pool;
        [JsonIgnore]
        private PluginManager plugMan;

        public MailSystem MailBoxes => mailBoxes;
        public KnowledgeBase KnowledgePool => pool;
    }
}