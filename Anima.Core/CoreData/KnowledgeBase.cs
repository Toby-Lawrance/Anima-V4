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
        public ConcurrentDictionary<string, TContainedType> Pool { get; }

        public KnowledgeBase()
        {
            Pool = new ConcurrentDictionary<string, TContainedType>();
        }

        public bool Exists(string id)
        {
            return Pool.ContainsKey(id);
        }

        //Inserting will only insert for null or non-existent values. This is safer for new things
        public bool TryInsertValue(string id, TContainedType val)
        {
            if (!Pool.ContainsKey(id) || !Pool[id]!.Equals(default(TContainedType)))
            {
                return Pool.TryAdd(id, val);
            }

            Pool[id] = val;
            return true;

        }

        public bool TrySetValue(string id, TContainedType val)
        {
            if (!Pool.ContainsKey(id))
            {
                return TryInsertValue(id,val);
            }

            var expectedVal = Pool[id];
            var result = Pool.AddOrUpdate(id,(k) => val,(id,kvp1) => expectedVal != null && expectedVal.Equals(kvp1) ? val : kvp1);
            return result != null && result.Equals(val);
        }

        //This just does it
        public void SetValue(string id, TContainedType val)
        {
            Pool[id] = val;
        }

        public bool TryGetValue(string id, out TContainedType? value)
        {
            if (Pool.ContainsKey(id))
            {
                value = Pool[id];
                return true;
            }

            value = default(TContainedType);
            return false;
        }
    }
    
}
