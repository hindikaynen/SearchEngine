using System.IO;

namespace SearchEngine
{
    public class TextField : Field
    {
        private readonly Stream _stream;

        public TextField(string name, Stream stream, FieldFlags flags) : base(name, flags)
        {
            _stream = stream;
        }

        public override StreamReader OpenReader()
        {
            return new StreamReader(_stream);
        }
    }
}
