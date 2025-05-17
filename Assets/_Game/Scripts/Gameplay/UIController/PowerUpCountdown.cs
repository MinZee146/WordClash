using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpCountdown : Singleton<PowerUpCountdown>
{
    [SerializeField] private float _time;
    [SerializeField] private GameObject _progressBar;
    [SerializeField] private Sprite _red, _green;

    private Tween _currentTween;

    private void OnEnable()
    {
        Notifier.Instance.PauseCountdown();
        if (_progressBar.GetComponent<Image>().fillAmount == 0)
        {
            BeginCountdown();
        }
        else
        {
            _currentTween?.Play();
        }
    }

    private void OnDisable()
    {
        Notifier.Instance.ResumeCountdown();
        _currentTween?.Pause();
    }

    private void BeginCountdown()
    {
        StopCountdown();
        _progressBar.SetActive(true);

        var isColorChanged = false;
        var image = _progressBar.GetComponent<Image>();
        image.sprite = _green;
        image.fillAmount = 1;
        image.DOKill();
        image.DOFade(1, 0);

        _currentTween = DOTween.To(() => image.fillAmount, x => image.fillAmount = x, 0, _time)
        .SetEase(Ease.Linear)
        .OnUpdate(() =>
        {
            if (!isColorChanged && image.fillAmount <= 0.3)
            {
                isColorChanged = true;
                image.sprite = _red;

                image.DOFade(0, 0.2f).OnComplete(() =>
                {
                    image.DOFade(1, 0.2f).SetLoops(-1, LoopType.Yoyo);
                });
            }
        })
        .OnComplete(() =>
        {
            PowerUpsManager.Instance.PlayerUseRandomPowerUp();
            UIManager.Instance.IsInspectingBoard = false;
        });
    }

    public void Reset()
    {
        _currentTween.Kill();
        _progressBar.GetComponent<Image>().fillAmount = 0;
    }

    private void StopCountdown()
    {
        _currentTween?.Kill();
        _progressBar.SetActive(false);
    }
}
