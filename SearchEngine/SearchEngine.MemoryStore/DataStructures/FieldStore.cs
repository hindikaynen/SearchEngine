using System.Collections.Concurrent;

namespace SearchEngine.MemoryStore
{
    class FieldStore
    {
        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, string>> _store = new ConcurrentDictionary<long, ConcurrentDictionary<string, string>>();

        public string GetValue(long docId, string fieldName)
        {
            ConcurrentDictionary<string, string> fields;
            if (!_store.TryGetValue(docId, out fields))
                return null;
            string value;
            if (!fields.TryGetValue(fieldName, out value))
                return null;
            return value;
        }

        public void SetValue(long docId, string fieldName, string value)
        {
            var fields = _store.GetOrAdd(docId, d => new ConcurrentDictionary<string, string>());
            fields[fieldName] = value;
        }
    }
}
