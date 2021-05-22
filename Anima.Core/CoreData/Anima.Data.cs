﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.CoreData;
using Core.PluginManagement;
using Newtonsoft.Json;

namespace Core
{
    public partial class Anima {

        private MailSystem mailBoxes;
        [JsonInclude]
        private PluginManager plugMan;

        public MailSystem MailBoxes => mailBoxes;
    }
}