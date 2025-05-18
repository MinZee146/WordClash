using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIController : Singleton<GameUIController>
{
    [SerializeField] private GameObject _hintButton, _confirmButton;
    [SerializeField] private RectTransform _boardRectTransform;
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private Image _background, _avatar;

    private Coroutine _shakeHintCoroutine;
    private Coroutine _shakeConfirmCoroutine;
    public CanvasGroup GameplayCanvasGroup;

    private void OnEnable()
    {
        StartHintShakeRoutine();
        StartConfirmShakeRoutine();
        ApplyTheme();
        GetAvatar();
    }

    private void OnDisable()
    {
        StopHintShakeRoutine();
        StopConfirmShakeRoutine();
    }

    private void ApplyTheme()
    {
        _background.sprite = ThemeManager.Instance.CurrentTheme.Background;
    }

    private void GetAvatar()
    {
        if (SceneManager.GetActiveScene().name == "TimeChallengeMode") return;

        var avatarKey = PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_CURRENT_AVATAR);

        Addressables.LoadAssetAsync<Sprite>(avatarKey).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _avatar.sprite = handle.Result;
            }
            else
            {
                Utils.LogError($"Failed to load avatar from address: {avatarKey}");
            }
        };
    }

    public void UpdateRoundIndicator()
    {
        _roundText.text = _roundText.text == "Round 1" ? "Round 2" : "Round 1";
    }

    public RectTransform ConfirmButtonRect()
    {
        return _confirmButton.GetComponent<RectTransform>();
    }

    public RectTransform BoardRectTransform()
    {
        return _boardRectTransform;
    }

    public void ToggleHintAndConfirm(bool hintState = true, bool display = true)
    {
        _hintButton.transform.DOKill();
        _confirmButton.transform.DOKill();

        if (!display || (!GameFlowManager.Instance.IsPlayerTurn && SceneManager.GetActiveScene().name != "TimeChallengeMode"))
        {
            _hintButton.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => _hintButton.SetActive(false));
            _confirmButton.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => _confirmButton.SetActive(false));

            return;
        }

        if (hintState)
        {
            _hintButton.transform.DORotate(Vector3.zero, 0.1f).SetEase(Ease.InOutSine);
            _hintButton.SetActive(true);
            _hintButton.transform.DOScale(Vector3.one, 0.15f);
            _confirmButton.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => _confirmButton.SetActive(false));
        }
        else
        {
            _confirmButton.SetActive(true);
            _confirmButton.transform.DOScale(Vector3.one, 0.15f);
            _hintButton.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => _hintButton.SetActive(false));
        }
    }

    public void OpenSettings()
    {
        PopUpsManager.Instance.ToggleSettingsPopUp(true);
    }

    private void StartHintShakeRoutine()
    {
        StopHintShakeRoutine();
        _shakeHintCoroutine = StartCoroutine(HintShakeAfterDelay());
    }

    private void StopHintShakeRoutine()
    {
        if (_shakeHintCoroutine != null)
        {
            StopCoroutine(_shakeHintCoroutine);
            _shakeHintCoroutine = null;
        }
    }

    private IEnumerator HintShakeAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        _hintButton.transform.DORotate(new Vector3(0, 0, 5), 0.25f)
        .From(new Vector3(0, 0, -5))
        .SetLoops(2, LoopType.Yoyo)
        .SetEase(Ease.InOutSine)
        .OnComplete(() =>
        {
            _hintButton.transform.DORotate(Vector3.zero, 0.1f).SetEase(Ease.InOutSine);
        });

        StartHintShakeRoutine();
    }

    private void StartConfirmShakeRoutine()
    {
        StopConfirmShakeRoutine();
        _shakeConfirmCoroutine = StartCoroutine(ConfirmShakeAfterDelay());
    }

    private void StopConfirmShakeRoutine()
    {
        if (_shakeConfirmCoroutine != null)
        {
            StopCoroutine(_shakeConfirmCoroutine);
            _shakeConfirmCoroutine = null;
        }
    }

    private IEnumerator ConfirmShakeAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        _confirmButton.transform.DOScale(new Vector3(1.15f, 1.15f, 1), 0.5f)
        .From(Vector3.one)
        .SetLoops(2, LoopType.Yoyo)
        .SetEase(Ease.InOutQuad)
        .OnComplete(() =>
        {
            _confirmButton.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);
        });

        StartConfirmShakeRoutine();
    }
}
