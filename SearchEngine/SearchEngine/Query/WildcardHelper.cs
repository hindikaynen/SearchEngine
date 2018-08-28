namespace SearchEngine
{
    static class WildcardHelper
    {
        private const char Star = '*';
        private const char QuestionMark = '?';

        public static bool IsWildcardQuery(string query)
        {
            return query.IndexOfAny(new[] {Star, QuestionMark}) != -1;
        }
    }
}
