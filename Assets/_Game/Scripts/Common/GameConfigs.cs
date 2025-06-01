using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigs", menuName = "ScriptableObjects/GameConfigs")]
public class GameConfigs : ScriptableObject
{
    [Header("Gameplay")]
    public double AIDifficulty;
    public int InitialHints;
    public int InitialCoins;
    public int CoinsPerGame;
}
