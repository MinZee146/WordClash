using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using DamageNumbersPro;
using MEC;
using System.Collections.Generic;
using System;

public class BottomBar : Singleton<BottomBar>
{
    [SerializeField] private GameObject _bonusScore;
    [SerializeField] private GameObject _freeze, _shuffle;
    [SerializeField] private PowerUpBase[] _sidePowerUp;

    private Coroutine _shakeCoroutine;
    private bool _usedShuffle, _usedFreeze;

    public void ToggleShuffle(bool state)
    {
        _shuffle.GetComponent<Button>().interactable = state;
        _shuffle.transform.GetChild(0).GetComponent<Image>().DOFade(state ? 1 : 0.5f, 0);
    }

    public void ToggleFreeze(bool state)
    {
        _freeze.GetComponent<Button>().interactable = state;
        _freeze.transform.GetChild(0).GetComponent<Image>().DOFade(state ? 1 : 0.5f, 0);
    }

    public void Reset()
    {
        _usedShuffle = false;
        _usedFreeze = false;

        ToggleShuffle(true);
        ToggleFreeze(true);
    }

    private void SpawnBonusScorePopUp(RectTransform rect)
    {
        _bonusScore.GetComponent<DamageNumber>().SpawnGUI(rect, new Vector2(0, 20f), 30);
    }

    public void ApplySidePowerUp(string powerUpName)
    {
        var selectedPowerUp = _sidePowerUp
           .Where(p => p.GetName() == powerUpName).FirstOrDefault(); ;

        if (selectedPowerUp.GetName() == "Shuffle")
        {
            _usedShuffle = true;
            ToggleShuffle(false);
        }

        if (selectedPowerUp.GetName() == "TimeFreeze")
        {
            _usedFreeze = true;
            ToggleFreeze(false);
        }

        AudioManager.Instance.PlaySFX("PowerupSelect");
        Notifier.Instance.OnUsePowerUp(selectedPowerUp.GetName());
        selectedPowerUp.ApplyPowerUp();
    }

    public IEnumerator<float> CheckForBonusScore()
    {
        if (!_usedShuffle)
        {
            SpawnBonusScorePopUp(_shuffle.GetComponent<RectTransform>());
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(PlayerStatsManager.Instance.UpdatePlayerScore(30)));
        }

        if (!_usedFreeze)
        {
            SpawnBonusScorePopUp(_freeze.GetComponent<RectTransform>());
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(PlayerStatsManager.Instance.UpdatePlayerScore(30)));
        }

        for (var i = 0; i < AI.Instance.GetUnusedPowerupsCount(); i++)
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(PlayerStatsManager.Instance.UpdateOpponentScore(30)));
        }
    }

    public void SetSidePowerUpState(bool state)
    {
        if (!_usedShuffle)
        {
            ToggleShuffle(state);
        }

        if (!_usedFreeze)
        {
            ToggleFreeze(state);
        }
    }

    public void StartShakeRoutine()
    {
        StopShakeRoutine();
        _shakeCoroutine = StartCoroutine(ShakeAfterDelay());
    }

    public void StopShakeRoutine()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }
    }

    private IEnumerator ShakeAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (!_usedShuffle)
        {
            PerformShake(_shuffle.transform);
        }

        if (!_usedFreeze)
        {
            PerformShake(_freeze.transform);
        }

        StartShakeRoutine();
    }

    private void PerformShake(Transform targetTransform)
    {
        targetTransform.DORotate(new Vector3(0, 0, 5), 0.25f)
            .From(new Vector3(0, 0, -5))
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                targetTransform.DORotate(Vector3.zero, 0.1f).SetEase(Ease.InOutSine);
            });
    }
}
