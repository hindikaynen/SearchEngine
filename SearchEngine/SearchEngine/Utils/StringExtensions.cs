using System.IO;
using System.Text;

namespace SearchEngine
{
    static class StringExtensions
    {
        public static StreamReader AsStreamReader(this string str)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return new StreamReader(ms);
        }
    }
}
