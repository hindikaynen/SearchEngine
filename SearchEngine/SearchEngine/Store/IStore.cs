using System;
using System.Collections.Generic;

namespace SearchEngine
{
    public interface IStore : IDisposable
    {
        long NextDocId();

        void AddPosting(string fieldName, string token, long docId);
        IEnumerable<long> GetPostings(string fieldName, string token);
        void RemoveDocument(long docId);

        void SetStoredFieldValue(long docId, string fieldName, string value);
        string GetStoredFieldValue(long docId, string fieldName);

        IEnumerable<string> WildcardSearch(string pattern);        
    }
}
