using UnityEngine;
using System.Collections.Generic;
using MEC;

public class GameManager : SingletonPersistent<GameManager>
{
    public bool IsGameOver { get; private set; }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        AudioManager.Instance.Initialize();
        UIManager.Instance.Initialize();
        CurrencyManager.Instance.Initialize();
        RewardManager.Instance.Initialize();
        ThemeManager.Instance.Initialize();
        PlayerDataTracker.Instance.Initialize();
        GameDictionary.Instance.Initialize();
    }

    public void NewGame()
    {
        IsGameOver = false;

        GameFlowManager.Instance.StartGame();
        BottomBar.Instance.SetSidePowerUpState(true);

        PlayerStatsManager.Instance.ResetStats();
        HintCounter.Instance.Reset();
        Notifier.Instance.Reset();
    }

    public void Replay()
    {
        PopUpsManager.Instance.ToggleGameOverPopUp(false);
        RewardManager.Instance.TotalAdDuration = 0;

        LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
        {
            NewGame();
            Board.Instance.NewGame();

            var totalMatchPlayed = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_TOTAL_MATCH_PLAYED, 0);
            LoadingAnimation.Instance.AnimationLoaded(0.5f, 0);
        });
    }

    public IEnumerator<float> CheckForGameOver()
    {
        WordFinder.Instance.FindAllWords();

        while (WordFinder.Instance.IsFindingWords)
        {
            yield return Timing.WaitForOneFrame;
        }

        IsGameOver = Board.Instance.FoundWords.Keys.Count == 0;

        if (IsGameOver)
        {
            Notifier.Instance.OnRoundChanged();
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySFX("TimeOut");

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(BottomBar.Instance.CheckForBonusScore()));

            yield return Timing.WaitForSeconds(1f);

            GameFlowManager.Instance.HandleGameOver();
        }
    }
}
