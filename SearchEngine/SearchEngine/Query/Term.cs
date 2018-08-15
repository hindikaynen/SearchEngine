namespace SearchEngine
{
    public class Term
    {
        public string FieldName { get; }

        public string Value { get; }

        public Term(string fieldName, string value)
        {
            FieldName = fieldName;
            Value = value;
        }
    }
}
