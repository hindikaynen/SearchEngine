using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DirectoryIndexer
{
    static class ConcurrentDictionaryExtensions
    {
        public static void RemoveByKeyValue<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            var collection = (ICollection<KeyValuePair<TKey, TValue>>) dictionary;
            collection.Remove(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
