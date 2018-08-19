using System;
using System.Collections.Generic;

namespace SearchEngine
{
    public interface ISearchIndex : IDisposable
    {
        void AddDocument(Document document);
        void RemoveDocument(Term term);

        List<long> Search(IQuery query);
        string GetFieldValue(long docId, string fieldName);
    }
}