using UnityEngine;

public abstract class PowerUpBase : ScriptableObject
{
    public Sprite Sprite;
    public string Description;
    protected string Name;

    public abstract void ApplyPowerUp();
    public string GetName() => Name;
}
