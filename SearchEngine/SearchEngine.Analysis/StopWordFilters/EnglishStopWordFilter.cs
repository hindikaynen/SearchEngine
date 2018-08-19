using System.Collections.Generic;

namespace SearchEngine.Analysis
{
    public static class EnglishStopWordFilter
    {
        private static readonly HashSet<string> StopWords = new HashSet<string> {
            "a", "an", "and", "are", "as", "at", "be", "but", "by",
            "for", "if", "in", "into", "is", "it",
            "no", "not", "of", "on", "or", "such",
            "that", "the", "their", "then", "there", "these",
            "they", "this", "to", "was", "will", "with"
        };

        public static bool IsStopWord(string token)
        {
            return StopWords.Contains(token);
        }
    }
}
