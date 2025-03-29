using System.ComponentModel;
using DG.Tweening;
using Genix.MocaLib.Runtime.Services;
using MEC;
using UnityEngine;

public partial class SROptions
{
    [Category("Debug")]
    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [Category("Debug")]
    public void HandleGameOver()
    {
        Timing.KillCoroutines();
        DOTween.KillAll();

        Board.Instance.ResetUI();
        Board.Instance.ResetData();
        Board.Instance.TileList.Clear();

        Timing.RunCoroutine(GameManager.Instance.CheckForGameOver());
    }

    [Category("Debug")]
    public void ShowAdIntegrationDebugger()
    {
        MocaLib.Instance.AdManager.ShowIntegrationDebugger();
    }

    [Category("PopUps")]
    public void Great()
    {
        UIManager.Instance.InstantiatePopUps("hhh");
    }

    [Category("PopUps")]
    public void Amazing()
    {
        UIManager.Instance.InstantiatePopUps("hhhh");
    }

    [Category("PopUps")]
    public void Fabulous()
    {
        UIManager.Instance.InstantiatePopUps("hhhhh");
    }

    [Category("PopUps")]
    public void Spectacular()
    {
        UIManager.Instance.InstantiatePopUps("hhhhhhh");
    }

    [Category("PopUps")]
    public void BestWord()
    {
        UIManager.Instance.BestWordPopUp("HanCute");
    }

    [Category("Cheats")]
    public void AddCoins()
    {
        CurrencyManager.Instance.UpdateCoins(1000);
    }

    [Category("Cheats")]
    public void GrantHints()
    {
        var currentHint = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER, 5);
        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER, currentHint + 5);
        HintCounter.Instance.FetchHintPref();
    }
}
