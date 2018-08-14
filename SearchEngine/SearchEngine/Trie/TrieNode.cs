using System.Collections.Concurrent;

namespace SearchEngine.Trie
{
    class TrieNode
    {
        public char Key { get; }
        public bool IsTerminal => !string.IsNullOrEmpty(Word);
        public string Word { get; set; }
        public ConcurrentDictionary<char, TrieNode> Children { get; }

        public TrieNode(char key)
        {
            Key = key;
            Children = new ConcurrentDictionary<char, TrieNode>();
        }
    }
}
