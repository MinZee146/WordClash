using System.Collections.Generic;
using System.Linq;
using Coffee.UIEffects;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThemeSelector : Singleton<ThemeSelector>, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject _themesContainer, _themeSlotPrefab, _purchaseButton, _errorText;
    [SerializeField] private GameObject _lockPanel, _lock;
    [SerializeField] private Sprite _locked, _unlocked;
    [SerializeField] private Button _nextButton, _previousButton, _selectButton;
    [SerializeField] private TextMeshProUGUI _themeName, _themePrice;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Vector3 _pageStep;
    [SerializeField] private Ease _tweenType;
    [SerializeField] private float _dragHoldThreshold = 0.5f;
    [SerializeField] private float _tweenTime;

    private RectTransform _pageRect;
    private Vector3 _targetPosition;
    private List<Theme> _themeList;
    private int _maxPage;
    private int _currentPage;
    private float _dragHoldTimer;
    private float _dragThreshold;
    private bool _isDragging;
    private bool _pageMovedDuringDrag;

    private void Start()
    {
        _themeList = ThemeManager.Instance.AllThemes.OrderByDescending(theme => PlayerPrefs.GetInt($"{theme.Name} Unlocked", 0)).ToList();
        _maxPage = _themeList.Count;
        _pageRect = _themesContainer.GetComponent<RectTransform>();
        _targetPosition = _pageRect.localPosition;

        for (var i = 0; i < _themeList.Count; i++)
        {
            var themeSlot = Instantiate(_themeSlotPrefab, _themesContainer.transform);
            themeSlot.GetComponent<Image>().sprite = _themeList[i].Illustration;
        }

        SnapToCurrentTheme();
    }

    #region Movement
    public void OnBeginDrag(PointerEventData eventData)
    {
        _pageMovedDuringDrag = false;
        _isDragging = true;
        _dragHoldTimer = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _lockPanel.SetActive(false);
        if (!_isDragging) return;
        _dragHoldTimer += Time.deltaTime;

        if (!(_dragHoldTimer >= 0.5f)) return;
        SnapToClosestPage(eventData);
        _isDragging = false;
        _dragHoldTimer = 0f;
    }

    private void SnapToClosestPage(PointerEventData eventData)
    {
        var dragDistance = eventData.position.x - eventData.pressPosition.x;
        var isDraggingToLeft = dragDistance < 0;

        switch (isDraggingToLeft)
        {
            case true when _currentPage < _maxPage:
                NextPage();
                break;
            case false when _currentPage > 1:
                PreviousPage();
                break;
            default:
                MovePage();
                break;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        var dragDistance = eventData.position.x - eventData.pressPosition.x;
        var isDraggingToLeft = dragDistance < 0;
        var absoluteDragDistance = Mathf.Abs(dragDistance);

        if (!_pageMovedDuringDrag)
        {
            if (_dragHoldTimer >= _dragHoldThreshold)
            {
                switch (isDraggingToLeft)
                {
                    case true when _currentPage < _maxPage:
                        NextPage();
                        break;
                    case false when _currentPage > 1:
                        PreviousPage();
                        break;
                    default:
                        MovePage();
                        break;
                }
            }
            else
            {
                if (absoluteDragDistance > _dragThreshold)
                {
                    switch (isDraggingToLeft)
                    {
                        case true when _currentPage < _maxPage:
                            NextPage();
                            break;
                        case false when _currentPage > 1:
                            PreviousPage();
                            break;
                    }
                }
                else
                {
                    MovePage();
                }
            }
        }

        _pageMovedDuringDrag = false;
        _dragHoldTimer = 0f;
        _lockPanel.SetActive(true);
        UpdateStatus();
    }

    public void NextPage()
    {
        if (_currentPage >= _maxPage) return;

        _currentPage++;
        _targetPosition += _pageStep;
        MovePage();
    }

    public void PreviousPage()
    {
        if (_currentPage <= 1) return;

        _currentPage--;
        _targetPosition -= _pageStep;
        MovePage();
    }

    public void SnapToCurrentTheme()
    {
        _currentPage = _themeList.IndexOf(ThemeManager.Instance.CurrentTheme) + 1;
        _targetPosition = _pageRect.localPosition += _pageStep * (_currentPage - 1);

        _pageRect.DOLocalMove(_targetPosition, _tweenTime).SetEase(_tweenType);
        UpdateStatus();
        UpdateButton();
    }

    private void MovePage()
    {
        AudioManager.Instance.PlaySFX("Swoosh");

        _pageRect.DOLocalMove(_targetPosition, _tweenTime).SetEase(_tweenType);
        UpdateStatus();
        UpdateButton();
    }
    #endregion

    #region Update
    private void UpdateButton()
    {
        _previousButton.interactable = _currentPage > 1;
        _nextButton.interactable = _currentPage < _maxPage;
    }

    public void UpdateStatus()
    {
        if (_currentPage >= 1 && _currentPage <= _themeList.Count)
        {
            _themeName.text = _themeList[_currentPage - 1].Name;
            _themePrice.text = _themeList[_currentPage - 1].Price.ToString();
        }

        _lockPanel.SetActive(_currentPage != 1 && PlayerPrefs.GetInt($"{_themeName.text} Unlocked", 0) == 0);
        _purchaseButton.SetActive(_lockPanel.activeSelf);
        _selectButton.interactable = !_lockPanel.activeSelf && _themeName.text != ThemeManager.Instance.CurrentTheme.Name;
    }

    private IEnumerator<float> UnlockAnimation()
    {
        _canvasGroup.blocksRaycasts = false;
        _lock.GetComponent<Image>().sprite = _unlocked;
        _lock.GetComponent<UIEffectTweener>().enabled = true;
        _lockPanel.GetComponent<Image>().DOFade(0f, 1f);

        yield return Timing.WaitForSeconds(1f);
        PlayerPrefs.SetInt($"{_themeName.text} Unlocked", 1);

        _lockPanel.GetComponent<Image>().DOFade(0.6f, 0f);
        _lock.GetComponent<UIEffectTweener>().enabled = false;
        _lock.GetComponent<UIEffect>().transitionRate = 0;
        _lock.GetComponent<Image>().sprite = _locked;

        _lockPanel.SetActive(false);
        _purchaseButton.SetActive(false);

        _selectButton.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator<float> WaitForUnlockTheme()
    {
        _nextButton.interactable = false;
        _previousButton.interactable = false;
        _selectButton.interactable = false;
        _purchaseButton.SetActive(false);

        yield return Timing.WaitUntilDone(Timing.RunCoroutine(UnlockAnimation()));

        UpdateButton();
    }

    public void UnlockTheme()
    {
        var price = int.Parse(_themePrice.text);
        if (price <= PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_COINS))
        {
            Timing.RunCoroutine(WaitForUnlockTheme());
            CurrencyManager.Instance.UpdateCoins(-price);
            AudioManager.Instance.PlaySFX("Cashing");
        }
        else
        {
            Error();
        }
    }

    private void Error()
    {
        _errorText.GetComponent<TextMeshProUGUI>()?.DOKill();
        _errorText.GetComponent<TextMeshProUGUI>().DOFade(1f, 0f);
        _errorText.SetActive(true);
        _errorText.GetComponent<TextMeshProUGUI>().DOFade(0f, 1f).SetEase(Ease.InOutQuad).SetDelay(2f).OnComplete(() =>
        {
            _errorText.SetActive(false);
        });
    }

    public void ChangeTheme()
    {
        ThemeManager.Instance.CurrentTheme = _themeList.FirstOrDefault(theme => theme.name == _themeName.text);
        UpdateStatus();
        PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_CURRENT_THEME, ThemeManager.Instance.CurrentTheme.Name);
        PopUpsManager.Instance.ToggleThemePopUp(false);

        Utils.Log($"Current theme set to {ThemeManager.Instance.CurrentTheme.Name}");
    }
    #endregion
}
