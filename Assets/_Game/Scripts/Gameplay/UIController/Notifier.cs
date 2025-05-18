using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Notifier : Singleton<Notifier>
{
    [SerializeField] private float _time;
    [SerializeField] private GameObject _progressBar;
    [SerializeField] private TextMeshProUGUI _notifyText;
    [SerializeField] private TypewriterByCharacter _typewriter;

    private Tween _currentTween;
    private bool _isColorChanged;
    private bool _isFreeze;

    public void OnTurnChanged()
    {
        if (SceneManager.GetActiveScene().name == "TimeChallengeMode")
        {
            TimeChallengeCountdown();
            return;
        }

        _typewriter.ShowText(GameFlowManager.Instance.IsPlayerTurn ? $"Your turn" : $"Opponent's turn");

        if (!PowerUpsManager.Instance.CheckReplaceLetter)
        {
            BeginCountdown();
        }
    }

    public void OnRoundChanged()
    {
        _typewriter.ShowText("No words left!");
    }

    public void OnUsePowerUp(string powerUpName)
    {
        var isPlayerTurn = GameFlowManager.Instance.IsPlayerTurn;

        if (powerUpName == "ReplaceLetter" && isPlayerTurn)
        {
            _typewriter.ShowText("Select a letter to replace");
        }
        else
        {
            var user = isPlayerTurn ? PlayerStatsManager.Instance.PlayerName : PlayerStatsManager.Instance.OpponentName;
            var formattedPowerUpName = System.Text.RegularExpressions.Regex.Replace(powerUpName, "(?<!^)([A-Z])", " $1");

            if (isPlayerTurn)
            {
                _typewriter.ShowText($"{user} used <rainb f=0.5>{formattedPowerUpName}</rainb>");
            }
            else
            {
                _typewriter.ShowText($"{user} used <rainb f=0.5>{formattedPowerUpName}</rainb>");
            }
        }

        if (powerUpName == "TimeFreeze")
        {
            _isFreeze = true;
        }
    }

    public void BeginCountdown()
    {
        StopCountdown();
        _progressBar.SetActive(true);
        _isColorChanged = false;
        _isFreeze = false;

        var image = _progressBar.GetComponent<Image>();
        image.color = GameFlowManager.Instance.IsPlayerTurn ? Colors.FromHex("16CC00") : Colors.FromHex("D30008");
        image.fillAmount = 1;
        image.DOKill();
        image.DOFade(1, 0);

        if (GameFlowManager.Instance.IsPlayerTurn)
        {
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
        else
        {
            image.fillOrigin = (int)Image.OriginHorizontal.Right;
        }

        _currentTween = DOTween.To(() => image.fillAmount, x => image.fillAmount = x, 0, _time)
        .SetEase(Ease.Linear)
        .OnUpdate(() =>
        {
            if (!_isColorChanged && image.fillAmount <= 0.3)
            {
                _isColorChanged = true;

                image.color = Colors.FromHex("D30008");
                image.DOFade(0, 0.2f).OnComplete(() =>
                {
                    image.DOFade(1, 0.2f).SetLoops(-1, LoopType.Yoyo);
                });

                AudioManager.Instance.PlaySideAudio("ClockTicking");
                BottomBar.Instance.StartShakeRoutine();
            }
        })
        .OnComplete(() =>
        {
            Board.Instance.ResetUI();
            Board.Instance.ResetData();
            WordDisplay.Instance.UndisplayWordAndScore();

            PowerUpsManager.Instance.CleanPowerUp();
            GameFlowManager.Instance.NextTurn();
            AudioManager.Instance.PlaySFX("TimeOut");
        });
    }

    public void PauseCountdown()
    {
        if (_isColorChanged)
        {
            AudioManager.Instance.StopSideAudio();
        }

        _currentTween?.Pause();
    }

    public void ResumeCountdown()
    {
        if (_isColorChanged && !_isFreeze)
        {
            AudioManager.Instance.PlaySideAudio("ClockTicking");
        }

        if (!_isFreeze)
        {
            _currentTween?.Play();
        }
    }

    public void StopCountdown()
    {
        AudioManager.Instance.StopSideAudio();
        BottomBar.Instance.StopShakeRoutine();

        _currentTween?.Kill();
        _progressBar.SetActive(false);
    }

    public void TimeChallengeCountdown()
    {
        StopCountdown();
        _progressBar.SetActive(true);
        _isColorChanged = false;

        var image = _progressBar.GetComponent<Image>();
        image.color = Colors.FromHex("16CC00");
        image.fillAmount = 1;
        image.DOKill();
        image.DOFade(1, 0);
        image.fillOrigin = (int)Image.OriginHorizontal.Left;

        _currentTween = DOTween.To(() => image.fillAmount, x => image.fillAmount = x, 0, 15)
        .SetEase(Ease.Linear)
        .OnUpdate(() =>
        {
            if (!_isColorChanged && image.fillAmount <= 0.3)
            {
                _isColorChanged = true;

                image.color = Colors.FromHex("D30008");
                image.DOFade(0, 0.2f).OnComplete(() =>
                {
                    image.DOFade(1, 0.2f).SetLoops(-1, LoopType.Yoyo);
                });

                AudioManager.Instance.PlaySideAudio("ClockTicking");
                BottomBar.Instance.StartShakeRoutine();
            }
        })
        .OnComplete(() =>
        {
            TimeChallengeMode.Instance.ResetUI();
            TimeChallengeMode.Instance.ResetData();
            WordDisplay.Instance.UndisplayWordAndScore();
            AudioManager.Instance.PlaySFX("TimeOut");
        });
    }
}
