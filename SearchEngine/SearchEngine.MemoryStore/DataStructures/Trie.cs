using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.MemoryStore
{
    class Trie
    {
        public const char Star = '*';
        public const char QuestionMark = '?';

        private readonly TrieNode _root;

        public Trie()
        {
            _root = new TrieNode(char.MinValue);
        }

        public void Add(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(nameof(value));
            
            var current = _root;
            foreach (var c in value)
            {
                var child = current.Children.GetOrAdd(c, key => new TrieNode(key));
                current = child;
            }
            current.Word = value;
        }

        public IEnumerable<string> WildcardSearch(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException(nameof(pattern));

            IEnumerable<TrieNode> current = new[] { _root };
            foreach (var c in pattern)
            {
                if (c == QuestionMark)
                {
                    current = current.SelectMany(node => node.Children.Values);
                    continue;
                }
                if (c == Star)
                {
                    current = current.SelectMany(TraverseAll);
                    continue;
                }
                current = current.Select(x =>
                {
                    TrieNode child;
                    if (x.Children.TryGetValue(c, out child))
                        return child;
                    return null;
                }).Where(x => x != null);
            }
            return current.Where(x => x.IsTerminal).Select(x => x.Word);
        }

        private IEnumerable<TrieNode> TraverseAll(TrieNode root)
        {
            yield return root;
            foreach (var child in root.Children.Values)
            {
                foreach (var node in TraverseAll(child))
                {
                    yield return node;
                }
            }
        }
    }
}
