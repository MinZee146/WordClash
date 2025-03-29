using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameDictionary : SingletonPersistent<GameDictionary>
{
    private HashSet<string> _words = new();
    private Trie _wordTrie = new();
    private TextAsset _dictText;

    public void Initialize()
    {
        Addressables.LoadAssetAsync<TextAsset>("ospd").Completed += OnDictionaryLoaded;
    }

    private void OnDictionaryLoaded(AsyncOperationHandle<TextAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _dictText = handle.Result;

            // Use StringReader to read the text
            using var reader = new StringReader(_dictText.text);
            while (reader.ReadLine() is { } line)
            {
                _words.Add(line.ToUpper());
                _wordTrie.Insert(line.ToUpper());
            }

            Utils.Log("Dictionary loaded and processed.");
        }
        else
        {
            Utils.LogError("Failed to load dictionary asset from Addressables.");
        }
    }

    public bool CheckWord(string word)
    {
        if (word == null)
        {
            return false;
        }

        return _words.Contains(word.ToUpper());
    }

    public bool IsPrefix(string prefix)
    {
        return _wordTrie.IsPrefix(prefix.ToUpper());
    }

    public void UnloadDictionary()
    {
        Addressables.Release(_dictText);
    }
}
