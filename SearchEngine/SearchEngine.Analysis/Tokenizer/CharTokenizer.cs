using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SearchEngine.Analysis
{
    public abstract class CharTokenizer
    {
        private const int BufferSize = 1024;

        protected abstract bool IsTokenChar(char c);

        public IEnumerable<string> Tokenize(StreamReader reader)
        {
            var sb = new StringBuilder();
            var buffer = new char[BufferSize];
            while (true)
            {
                int readChars = reader.Read(buffer, 0, BufferSize);
                if(readChars == 0)
                    break;

                for (int i = 0; i < readChars; i++)
                {
                    char current = buffer[i];
                    if (!IsTokenChar(current))
                    {
                        if (sb.Length > 0)
                            yield return sb.ToString();
                        sb.Clear();
                        continue;
                    }
                    sb.Append(current);
                }
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }
    }
}
