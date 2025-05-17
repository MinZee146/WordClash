using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingAnimation : SingletonPersistent<LoadingAnimation>
{
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private RectTransform _leftPanel, _rightPanel, _topPanel;
    [SerializeField] private CanvasScaler _canvasScaler;

    private Vector2 leftOriginalPos;
    private Vector2 rightOriginalPos;
    private Vector2 topOriginalPos;

    private float screenWidth;
    private float screenHeight;

    public void Initialize()
    {
        var referenceResolution = _canvasScaler.referenceResolution;
        var matchWidthOrHeight = _canvasScaler.matchWidthOrHeight;

        var widthScale = Screen.width / referenceResolution.x;
        var heightScale = Screen.height / referenceResolution.y;
        var scaleFactor = Mathf.Lerp(widthScale, heightScale, matchWidthOrHeight);

        screenWidth = 1.25f * Screen.width / scaleFactor;
        screenHeight = 1.25f * Screen.height / scaleFactor;

        _leftPanel.sizeDelta = new Vector2(screenWidth / 2f, screenHeight);
        _rightPanel.sizeDelta = new Vector2(screenWidth / 2f, screenHeight);
        _topPanel.sizeDelta = new Vector2(screenWidth, screenHeight / 5f);

        leftOriginalPos = new Vector2(-screenWidth / 2f, 0);
        rightOriginalPos = new Vector2(screenWidth / 2f, 0);
        topOriginalPos = new Vector2(0, screenHeight / 5f);

        _leftPanel.anchoredPosition = leftOriginalPos;
        _rightPanel.anchoredPosition = rightOriginalPos;
        _topPanel.anchoredPosition = topOriginalPos;

        _loadingPanel.SetActive(false);
    }

    public void AnimationLoading(float transitionDuration, Action onComplete = null)
    {
        _loadingPanel.SetActive(true);

        var closeSequence = DOTween.Sequence();
        closeSequence.Join(_topPanel.DOAnchorPos(Vector2.zero, transitionDuration));
        closeSequence.Join(_leftPanel.DOAnchorPos(Vector2.zero, transitionDuration));
        closeSequence.Join(_rightPanel.DOAnchorPos(Vector2.zero, transitionDuration));
        closeSequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            AudioManager.Instance.StopMusic();
        });
    }

    public void AnimationLoaded(float transitionDuration, float delay)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            AudioManager.Instance.PlaySFX("Bell");
            var openSequence = DOTween.Sequence();

            openSequence.Join(_topPanel.DOAnchorPos(topOriginalPos, transitionDuration));
            openSequence.Join(_leftPanel.DOAnchorPos(leftOriginalPos, transitionDuration));
            openSequence.Join(_rightPanel.DOAnchorPos(rightOriginalPos, transitionDuration));
            openSequence.OnComplete(() =>
            {
                _loadingPanel.SetActive(false);

                if (SceneManager.GetActiveScene().name == "Gameplay")
                {
                    if (GameFlowManager.Instance.Round == 1)
                    {
                        AudioManager.Instance.PlayMusic("Round1");
                    }
                    else
                    {
                        AudioManager.Instance.PlayMusic("Round2");
                    }
                }
                else
                {
                    AudioManager.Instance.PlayMusic("Menu");
                }
            });
        });
    }
}