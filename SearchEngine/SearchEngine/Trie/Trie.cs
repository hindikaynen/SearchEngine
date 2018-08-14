using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Trie
{
    class Trie
    {
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

        public IEnumerable<string> SearchByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(nameof(prefix));

            var current = _root;
            foreach (var c in prefix)
            {
                if (!current.Children.TryGetValue(c, out current))
                    return Enumerable.Empty<string>();
            }

            return GetLeafs(current);
        }

        private IEnumerable<string> GetLeafs(TrieNode node)
        {
            var stack = new Stack<TrieNode>();
            stack.Push(node);
            while (stack.Any())
            {
                var current = stack.Pop();
                if (current.IsTerminal)
                    yield return current.Word;

                foreach (var child in current.Children.Values)
                {
                    stack.Push(child);
                }
            }
        }
    }
}
