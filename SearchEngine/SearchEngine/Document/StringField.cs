using System.IO;
using System.Text;

namespace SearchEngine
{
    public class StringField : Field
    {
        private readonly string _value;

        public StringField(string name, string value, FieldFlags flags) : base(name, flags)
        {
            _value = value;
        }

        public override StreamReader OpenReader()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(_value));
            return new StreamReader(memoryStream);
        }
    }
}
