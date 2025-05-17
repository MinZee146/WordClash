using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RewardManager : SingletonPersistent<RewardManager>
{
    [NonSerialized] public int TotalAdDuration;
    [SerializeField] private GameObject _dailySpinNotifier, _dailyAdButton;
    [SerializeField] private TextMeshProUGUI _dailyAdCount;

    public void Initialize()
    {
        GrantDailySpin();
        ResetDailyAd();
    }

    public void GrantDailySpin()
    {
        var lastRewardTimestamp = PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_LAST_DAILY_REWARD, "");

        if (!DateTime.TryParse(lastRewardTimestamp, out var lastRewardDate))
        {
            lastRewardDate = DateTime.MinValue;
        }

        var now = DateTime.Now;
        var timeSinceLastReward = now - lastRewardDate;

        if (timeSinceLastReward.TotalHours > 24 || !PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_HAS_SPUN_TODAY))
        {
            PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_LAST_DAILY_REWARD, now.ToString());
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_HAS_SPUN_TODAY, 0);
        }

        var hasSpunToday = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_HAS_SPUN_TODAY, 0) == 1;

        _dailySpinNotifier.SetActive(!hasSpunToday);
    }

    public void DisableSpin()
    {
        _dailySpinNotifier.SetActive(false);
        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_HAS_SPUN_TODAY, 1);
    }

    public void ResetDailyAd()
    {
        var lastAdDate = PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_LAST_DAILY_AD_RESET);
        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastAdDate != currentDate)
        {
            PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_LAST_DAILY_AD_RESET, currentDate);
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_DAILY_AD_COUNT, 5);
        }

        _dailyAdCount.text = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_DAILY_AD_COUNT).ToString();

#if UNITY_EDITOR
        SetAdCoinsButton(PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_DAILY_AD_COUNT) > 0);
#endif
    }

    public void GrantHints(GameObject warning)
    {
        if (PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_COINS) >= 100)
        {
            CurrencyManager.Instance.UpdateCoins(-100);
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER, 5);
            HintCounter.Instance.FetchHintPref();
            PopUpsManager.Instance.ToggleMoreHintsPopUp(false);
        }
        else
        {
            warning.GetComponent<TextMeshProUGUI>().DOKill();
            warning.GetComponent<TextMeshProUGUI>().DOFade(1f, 0f);
            warning.SetActive(true);
            warning.GetComponent<TextMeshProUGUI>().DOFade(0f, 1f).SetEase(Ease.InOutQuad).SetDelay(2f).OnComplete(() =>
            {
                warning.SetActive(false);
            });
        }
    }

    public void SetAdCoinsButton(bool state)
    {
        _dailyAdButton.GetComponent<CanvasGroup>().interactable = state;
        _dailyAdButton.GetComponent<CanvasGroup>().alpha = state ? 1f : 0.8f;
    }
}
