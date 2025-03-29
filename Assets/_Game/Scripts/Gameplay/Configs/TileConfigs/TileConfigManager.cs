using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TileConfigManager
{
    public event Action HandleConfigsLoaded;
    public TileConfig[] Configs => _tileConfigList;

    private TileConfig[] _tileConfigList;
    private AsyncOperationHandle<IList<TileConfig>> _loadedConfigsHandle;
    private readonly Dictionary<char, float> _letterFrequency = new()
    {
        {'E', 11.1607f}, {'A', 8.4966f}, {'I', 7.5448f}, {'O', 7.1635f}, {'N', 6.6544f}, {'R', 7.5809f}, {'T', 6.9509f},
        {'L', 5.7351f}, {'S', 5.7351f}, {'U', 3.6308f}, {'D', 3.3844f}, {'G', 2.4705f}, {'B', 2.0720f}, {'C', 4.5388f},
        {'M', 3.0129f}, {'P', 3.1671f}, {'F', 1.8121f}, {'H', 3.0034f}, {'V', 1.0074f}, {'W', 1.2899f}, {'Y', 1.7779f},
        {'K', 1.1016f}, {'X', 0.2902f}, {'Z', 0.2722f}, {'Q', 0.1962f}, {'J', 0.1965f}
    };

    public TileConfig GetRandomLetter()
    {
        var totalWeight = _letterFrequency.Sum(entry => entry.Value);
        var randomWeight = UnityEngine.Random.Range(0, totalWeight);

        foreach (var entry in _letterFrequency)
        {
            if (randomWeight < entry.Value)
            {
                return _tileConfigList.FirstOrDefault(tileStat => tileStat.Letter == entry.Key);
            }

            randomWeight -= entry.Value;
        }

        return null;
    }

    public TileConfig GetConfig(char letter)
    {
        return _tileConfigList?.FirstOrDefault(tileStat => tileStat.Letter == letter);
    }

    public void LoadConfigs()
    {
        // Load all TileConfig assets with the label "TileConfigs" asynchronously
        _loadedConfigsHandle = Addressables.LoadAssetsAsync<TileConfig>("Tile", null);
        _loadedConfigsHandle.Completed += OnConfigsLoaded;
    }

    private void OnConfigsLoaded(AsyncOperationHandle<IList<TileConfig>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _tileConfigList = handle.Result.ToArray();
            HandleConfigsLoaded?.Invoke();
        }
        else
        {
            Utils.LogError("Failed to load TileConfigs from Addressables.");
        }
    }

    public void UnloadConfigs()
    {
        if (_loadedConfigsHandle.IsValid())
        {
            Addressables.Release(_loadedConfigsHandle);
            _tileConfigList = null;
        }
    }
}
