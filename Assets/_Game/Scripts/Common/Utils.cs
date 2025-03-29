using UnityEngine;

public class Utils
{
    public static void Log(string s)
    {
#if UNITY_EDITOR
        Debug.Log(s);
#endif
    }

    public static void LogWarning(string s)
    {
#if UNITY_EDITOR
        Debug.LogWarning(s);
#endif
    }

    public static void LogError(string s)
    {
#if UNITY_EDITOR
        Debug.LogError(s);
#endif
    }
}
