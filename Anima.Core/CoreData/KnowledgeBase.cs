using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Anima.Core.CoreData
{
    public class KnowledgeBase
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string,object>> _pool;

        public ConcurrentDictionary<string, ConcurrentDictionary<string, object>> Pool => _pool;

        public KnowledgeBase()
        {
            _pool = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
        }
    }
}
