using System.Collections.Generic;

public class TrieNode
{
    public Dictionary<char, TrieNode> Children = new();
    public bool IsWord = false;
}

public class Trie
{
    private readonly TrieNode _root = new();

    public void Insert(string word)
    {
        var current = _root;

        foreach (var letter in word)
        {
            if (!current.Children.ContainsKey(letter))
            {
                current.Children[letter] = new();
            }

            current = current.Children[letter];
        }

        current.IsWord = true;
    }

    public bool IsWord(string word)
    {
        var node = GetNode(word);

        return node != null && node.IsWord;
    }

    public bool IsPrefix(string prefix)
    {
        return GetNode(prefix) != null;
    }

    private TrieNode GetNode(string word)
    {
        var current = _root;

        foreach (var letter in word)
        {
            if (!current.Children.ContainsKey(letter))
            {
                return null;
            }

            current = current.Children[letter];
        }

        return current;
    }
}