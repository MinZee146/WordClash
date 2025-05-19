using UnityEngine;
using System.Collections.Generic;
using MEC;
using UnityEngine.SceneManagement;

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
        PlayerStatsManager.Instance.ResetStats();
    }

    public void Replay()
    {
        PopUpsManager.Instance.ToggleGameOverPopUp(false);
        LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
        {
            if (SceneManager.GetActiveScene().name == "TimeChallengeMode")
            {
                PlayerStatsManager.Instance.ResetStats();
                TimeChallengeMode.Instance.NewGame();
            }
            else
            {
                NewGame();
                Board.Instance.NewGame();
            }

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

    public IEnumerator<float> CheckForRefill()
    {
        WordFinder.Instance.FindAllWords();

        while (WordFinder.Instance.IsFindingWords)
        {
            yield return Timing.WaitForOneFrame;
        }

        var needRefill = TimeChallengeMode.Instance.FoundWords.Keys.Count == 0;

        if (needRefill)
        {
            TimeChallengeMode.Instance.GenerateBoard();
        }
    }
}
