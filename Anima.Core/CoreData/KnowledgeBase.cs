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

namespace Core.CoreData
{
    public class KnowledgeBase
    {
        [JsonInclude]
        private readonly ConcurrentDictionary<string, KeyValuePair<Type,object>> _pool;

        [JsonInclude]
        public ConcurrentDictionary<string, KeyValuePair<Type, object>> Pool => _pool;

        public KnowledgeBase()
        {
            _pool = new ConcurrentDictionary<string, KeyValuePair<Type, object>>();
        }

        public bool Exists(string id)
        {
            return _pool.ContainsKey(id);
        }

        //Inserting will only insert for null or non-existent values. This is safer for new things
        public bool TryInsertValue<T>(string id, T val)
        {

            if (_pool.ContainsKey(id) && _pool[id].Value.Equals(default(T)))
            {
                _pool[id] =new KeyValuePair<Type, object>(typeof(T), val);
                return true;
            }

            return _pool.TryAdd(id, new KeyValuePair<Type, object>(typeof(T), val));
        }

        //This just does it
        public void SetValue<T>(string id, T val)
        {
            var item = new KeyValuePair<Type, object>(typeof(T), val);
            _pool[id] = item;
        }

        public Type GetTypeOfValue(string id)
        {
            return _pool.ContainsKey(id) ? _pool[id].Key : null;
        }

        public bool TryGetValue<T>(string id, out T value)
        {

            if (_pool.ContainsKey(id))
            {
                try
                {
                    value = (T)(Convert.ChangeType(_pool[id].Value,_pool[id].Key));
                    return true;
                }
                catch (InvalidCastException e)
                {
                    Anima.Instance.ErrorStream.WriteLine($"Invalid Cast:{e.Message}");
                }
            }

            value = default(T);
            return false;
        }
    }
}
