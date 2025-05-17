using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigs", menuName = "ScriptableObjects/GameConfigs")]
public class GameConfigs : ScriptableObject
{
    [Header("Gameplay")]
    public double AIDifficulty;
    public int InitialHints;
    public int InitialCoins;
    public int CoinsPerAd;
    public int CoinsPerGame;

    [Header("Misc")]
    public bool CheatsEnabled;
    public bool InternetCheck;

    [Header("Rating")]
    public int RatingShowAtMatch;

    [Header("Ads")]
    public int AdsStartFromMatch;
}
