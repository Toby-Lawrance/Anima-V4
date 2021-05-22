using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Core.CoreData
{
    public class KnowledgeBase<TContainedType>
    {
        [JsonInclude]
        private readonly ConcurrentDictionary<string, TContainedType> _pool;

        [JsonInclude]
        public ConcurrentDictionary<string, TContainedType> Pool => _pool;

        public KnowledgeBase()
        {
            _pool = new ConcurrentDictionary<string, TContainedType>();
        }

        public bool Exists(string id)
        {
            return _pool.ContainsKey(id);
        }

        //Inserting will only insert for null or non-existent values. This is safer for new things
        public bool TryInsertValue(string id, TContainedType val)
        {
            if (!_pool.ContainsKey(id) || !_pool[id].Equals(default(TContainedType)))
            {
                return _pool.TryAdd(id, val);
            }

            _pool[id] = val;
            return true;

        }

        public bool TrySetValue(string id, TContainedType val)
        {
            if (!_pool.ContainsKey(id))
            {
                return TryInsertValue(id,val);
            }

            var expectedVal = _pool[id];
            var result = _pool.AddOrUpdate(id,(k) => val,(id,kvp1) => expectedVal.Equals(kvp1) ? val : kvp1);
            return result.Equals(val);
        }

        //This just does it
        public void SetValue(string id, TContainedType val)
        {
            _pool[id] = val;
        }

        public bool TryGetValue(string id, out TContainedType value)
        {
            if (_pool.ContainsKey(id))
            {
                value = _pool[id];
                return true;
            }

            value = default(TContainedType);
            return false;
        }
    }
    
}
