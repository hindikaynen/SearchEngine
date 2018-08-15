using System.Collections.Generic;

namespace SearchEngine
{
    public class Document
    {
        private readonly List<Field> _fields = new List<Field>();

        public IEnumerable<Field> Fields => _fields;

        public void AddField(Field field)
        {
            _fields.Add(field);
        }
    }
}
