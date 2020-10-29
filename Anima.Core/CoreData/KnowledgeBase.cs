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

        public bool TrySetValue<T>(string id, T val)
        {
            var item = new KeyValuePair<Type, object>(typeof(T), val);
            if (!_pool.ContainsKey(id))
            {
                return TryInsertValue(id,val);
            }

            var expectedVal = _pool[id];
            var result = _pool.AddOrUpdate(id,(k) => item,(id,kvp1) => expectedVal.Value.Equals(kvp1.Value) ? item : kvp1);
            return result.Value.Equals(item.Value);
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
            if (_pool.ContainsKey(id) && _pool[id].Value is T obj)
            {
                value = obj;
                return true;
            }

            value = default(T);
            return false;
        }

        public bool TryGetValue<T>(string id, out IEnumerable<T> value)
        {
            if (_pool.ContainsKey(id) && _pool[id].Value.GetType().IsArray)
            {
                value = ((object[])_pool[id].Value).Cast<T>();
                return true;
            }

            value = default(IEnumerable<T>);
            return false;
        }
    }

    public class MyTypedKeyValueConverter : Newtonsoft.Json.JsonConverter<KeyValuePair<Type,object>>
    {
        public override KeyValuePair<Type, object> ReadJson(JsonReader reader, Type objectType, KeyValuePair<Type, object> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var kvpConv = new KeyValuePairConverter();

            var obj = kvpConv.ReadJson(reader, objectType, existingValue, serializer);
            if (obj is null) { return default(KeyValuePair<Type,object>); }

            var (key, value) = (KeyValuePair<Type, object>)obj;
            
            if (value is IConvertible)
            {
                return new KeyValuePair<Type, object>(key, Convert.ChangeType(value, key));
            }
            else if (value is JArray jarr)
            {
                var arr = jarr.Select(jv => Convert.ChangeType(jv, key.GetElementType())).ToArray();
                return new KeyValuePair<Type, object>(key,arr);
            }

            return new KeyValuePair<Type, object>(key, value);
            
        }


        public override void WriteJson(JsonWriter writer, KeyValuePair<Type, object> value, JsonSerializer serializer)
        {
            var kvpConv = new KeyValuePairConverter();
            kvpConv.WriteJson(writer,value,serializer);
        }
    }
}
