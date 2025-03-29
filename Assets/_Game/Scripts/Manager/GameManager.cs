using UnityEngine;
using System.Collections.Generic;
using MEC;
using Genix.MocaLib.Runtime.Services;

public class GameManager : SingletonPersistent<GameManager>
{
    public bool IsGameOver { get; private set; }

    public enum GameMode
    {
        PvC,
        PvP,
    }
    public enum Location
    {
        Home,
        Gameplay,
    }

    public GameMode CurrentGameMode = GameMode.PvC;
    public Location CurrentLocation = Location.Home;

    public void Initialize()
    {
        AudioManager.Instance.Initialize();
        UIManager.Instance.Initialize();
        CurrencyManager.Instance.Initialize();
        PlayerDataTracker.Instance.Initialize();
        GameDictionary.Instance.Initialize();
    }

    public void InitializeAdSettings()
    {
        MocaLib.Instance.AdManager.RegisterOnAdImpressionEvent(data =>
        {
            MocaLib.Instance.AnalyticsManager.LogAdRevenue(CurrentGameMode.ToString(), 0, CurrentLocation.ToString(), data);
        });

        if (!PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_IS_ADS_ENABLED))
        {
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_IS_ADS_ENABLED, 1);
        }

        if (PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_IS_ADS_ENABLED) == 1)
        {
            MocaLib.Instance.AdManager.ShowBannerAd();
            NavigationBarController.Instance.UpdatePositionBasedOnAds();
        }

        MocaLib.Instance.AdManager.RegisterOnRewardedAdAvailabilityChangedEvent(isRewardedAdReady =>
        {
            DoubleRewardPopUp.Instance.IsRewardedAdReady = isRewardedAdReady;
            RewardManager.Instance.SetAdCoinsButton(isRewardedAdReady &&
                                                    PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_DAILY_AD_COUNT) > 0);
        });
    }

    public void NewGame()
    {
        MocaLib.Instance.AnalyticsManager.LogEvent("level_start", new Dictionary<string, object>
        {
            { "play_mode", CurrentGameMode }
        });

        IsGameOver = false;

        GameFlowManager.Instance.StartGame();
        BottomBar.Instance.SetSidePowerUpState(true);

        PlayerStatsManager.Instance.ResetStats();
        HintCounter.Instance.Reset();
        Notifier.Instance.Reset();
    }

    public void Replay()
    {
        MocaLib.Instance.AnalyticsManager.LogEvent("replay", new Dictionary<string, object>
        {
            { "play_mode", CurrentGameMode }
        });

        PopUpsManager.Instance.ToggleGameOverPopUp(false);
        RewardManager.Instance.TotalAdDuration = 0;

        LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
        {
            NewGame();
            Board.Instance.NewGame();

            var totalMatchPlayed = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_TOTAL_MATCH_PLAYED, 0);
            var shouldPlayAd = totalMatchPlayed >= RemoteConfigs.Instance.GameConfigs.AdsStartFromMatch;

            if (PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_IS_ADS_ENABLED) == 1 && shouldPlayAd)
            {
                UIManager.Instance.ToggleAdBreak(true);

                Timing.RunCoroutine(AdBreak.Instance.Countdown(() =>
                {
                    Notifier.Instance.StopCountdown();
                    UIManager.Instance.ToggleAdBreak(false);

                    MocaLib.Instance.AdManager.ShowInterstitialAd((isSuccessful, adDuration) =>
                    {
                        if (isSuccessful)
                        {
                            RewardManager.Instance.TotalAdDuration += adDuration;
                        }

                        Notifier.Instance.BeginCountdown();
                        LoadingAnimation.Instance.AnimationLoaded(0.5f, 0);
                    });
                }));
            }
            else
            {
                LoadingAnimation.Instance.AnimationLoaded(0.5f, 0);
            }
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
