namespace SearchEngine
{
    public class Posting
    {
        public long DocId { get; }

        public Posting(long docId)
        {
            DocId = docId;
        }
    }
}
