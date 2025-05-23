using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            var objs = FindObjectsByType<T>(FindObjectsSortMode.None);
            if (objs is { Length: > 0 })
                _instance = objs[0];

            if (objs is { Length: > 1 })
            {
                Utils.LogError("There is more than one " + typeof(T).Name + " in the scene.");
            }

            if (_instance != null) return _instance;

            var obj = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _instance = obj.AddComponent<T>();
            return _instance;
        }
    }
}


public class SingletonPersistent<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
