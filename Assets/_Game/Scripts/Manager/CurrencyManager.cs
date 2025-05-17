using System;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;

public class CurrencyManager : SingletonPersistent<CurrencyManager>
{
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private GameObject _coinsIcon, _coinAttraction;
    [SerializeField] private ParticleImage _particleCoins;

    public bool IsAnimating => _isAnimating;

    private int _currentAmount;
    private bool _isAnimating;

    public void Initialize()
    {
        FetchPrefs();
    }

    public void FetchPrefs()
    {
        _currentAmount = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_COINS, RemoteConfigs.Instance.GameConfigs.InitialCoins);
        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_COINS, _currentAmount);

        UpdateText();
    }

    private void UpdateText()
    {
        var formattedNumber = string.Format("{0:N0}", _currentAmount);
        _coinsText.text = formattedNumber.ToString();
    }

    public void UpdateCoins(int amount)
    {
        Timing.RunCoroutine(AnimateIncrease(amount));
        UpdateText();
    }

    private IEnumerator<float> AnimateIncrease(int amount)
    {
        _coinsIcon.transform.DOShakePosition(0.5f, 20f, 20, 75, false, true, ShakeRandomnessMode.Full);

        var startCoins = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_COINS);
        var targetCoins = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_COINS) + amount;
        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_COINS, targetCoins);

        var duration = 0.5f;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Timing.DeltaTime;
            var progress = elapsedTime / duration;
            var newCoins = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, progress));

            _currentAmount = newCoins;
            UpdateText();

            yield return Timing.WaitForOneFrame;
        }

        _currentAmount = targetCoins;
        UpdateText();
    }

    public void CoinsAttract(int amount, Vector3 startPosition, Action onParticleStop = null)
    {
        if (_isAnimating) return;
        _coinAttraction.transform.position = startPosition;

        _particleCoins.onParticleStarted.RemoveAllListeners();
        _particleCoins.onParticleStop.RemoveAllListeners();
        _particleCoins.onFirstParticleFinished.RemoveAllListeners();
        _particleCoins.onAnyParticleFinished.RemoveAllListeners();

        _particleCoins.onParticleStarted.AddListener(() =>
        {
            _isAnimating = true;
        });

        _particleCoins.onParticleStop.AddListener(() =>
        {
            _isAnimating = false;
            _coinAttraction.transform.position = Vector2.zero;
            onParticleStop?.Invoke();
        });

        _particleCoins.onFirstParticleFinished.AddListener(() =>
        {
            UpdateCoins(amount);
        });

        _particleCoins.onAnyParticleFinished.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("CollectCoin");
        });

        _particleCoins.Play();
    }
}
