using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "NewTheme")]
public class Theme : ScriptableObject
{
    public string Name;
    public float Price;
    public Sprite Illustration, Background;
}