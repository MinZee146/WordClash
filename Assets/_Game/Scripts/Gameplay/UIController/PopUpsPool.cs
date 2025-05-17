using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PopUpsPool : Singleton<PopUpsPool>
{
    [SerializeField] private Canvas _canvas;
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _addressableHandles = new();

    public void Instantiate()
    {
        SpawnFromPool("Great");
        SpawnFromPool("Amazing");
        SpawnFromPool("Fabulous");
        SpawnFromPool("Spectacular");
    }

    public void SpawnFromPool(string key)
    {
        if (!_poolDictionary.ContainsKey(key))
        {
            _poolDictionary[key] = new Queue<GameObject>();
        }

        if (_poolDictionary[key].Count > 0)
        {
            var obj = _poolDictionary[key].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            Addressables.LoadAssetAsync<GameObject>(key).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _addressableHandles[key] = handle;

                    var obj = Instantiate(handle.Result, _canvas.transform, false);
                    obj.GetComponent<PopUps>().SetKey(key);
                    ReturnToPool(key, obj);
                }
                else
                {
                    Utils.LogError($"Failed to load prefab with key: {key}");
                }
            };
        }
    }

    public void ReturnToPool(string key, GameObject obj)
    {
        if (!_poolDictionary.ContainsKey(key))
        {
            _poolDictionary[key] = new Queue<GameObject>();
        }

        obj.SetActive(false);
        _poolDictionary[key].Enqueue(obj);
    }

    private void OnDestroy()
    {
        foreach (var handle in _addressableHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}
