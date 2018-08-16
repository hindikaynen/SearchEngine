using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SearchEngine.Analysis
{
    public abstract class CharTokenizer
    {
        protected abstract bool IsTokenChar(char c);

        public IEnumerable<string> Tokenize(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();
            int current;
            while ((current = reader.Read()) != -1)
            {
                char c = (char) current;
                if (!IsTokenChar(c))
                {
                    if (sb.Length > 0)
                        yield return sb.ToString();
                    sb.Clear();
                    continue;
                }
                sb.Append(c);
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }
    }
}
