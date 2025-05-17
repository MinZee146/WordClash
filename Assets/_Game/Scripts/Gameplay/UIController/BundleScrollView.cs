using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BundleScollView : Singleton<BundleScollView>, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _pageRect;
    [SerializeField] private Vector3 _pageStep;
    [SerializeField] private Ease _tweenType;
    [SerializeField] private Sprite _barClose, _barOpen;
    [SerializeField] private Image[] _barImage;
    [SerializeField] private float _dragHoldThreshold = 0.5f;
    [SerializeField] private float _autoScrollInterval = 5f;
    [SerializeField] private float _tweenTime;

    private CoroutineHandle _autoScrollHandle;
    private Vector3 _targetPosition;
    private Vector3 _originalPosition;
    private GameObject _bundleScollView;
    private int _maxPage;
    private int _currentPage;
    private float _dragHoldTimer;
    private float _dragThreshold;
    private bool _isDragging;
    private bool _pageMovedDuringDrag;
    private bool _isAutoScrolling;
    private bool _wasAutoScrolling;

    private void Start()
    {
        _currentPage = 1;
        _maxPage = _pageRect.childCount;
        _targetPosition = _pageRect.localPosition;
        _originalPosition = _pageRect.localPosition;

        _isAutoScrolling = true;
        _bundleScollView = transform.parent.gameObject;

        UpdateBar();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData eventData = new(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);

            var clickedOnBundleScrollView = results.Exists(result =>
                result.gameObject.transform.IsChildOf(_bundleScollView.transform));

            if (clickedOnBundleScrollView)
            {
                _isAutoScrolling = false;
                StopAutoScroll();
            }
            else
            {
                _isAutoScrolling = true;

                if (!_wasAutoScrolling)
                {
                    StartAutoScroll();
                }
            }

            _wasAutoScrolling = _isAutoScrolling;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopAutoScroll();

        _pageMovedDuringDrag = false;
        _isDragging = true;
        _dragHoldTimer = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _dragHoldTimer += Time.deltaTime;

        if (!(_dragHoldTimer >= 0.5f)) return;
        SnapToClosestPage(eventData);
        _isDragging = false;
        _dragHoldTimer = 0f;
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

        if (_isAutoScrolling)
        {
            StartAutoScroll();
        }
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

    private void MovePage()
    {
        _pageRect.DOLocalMove(_targetPosition, _tweenTime).SetEase(_tweenType);
        UpdateBar();
    }

    private void UpdateBar()
    {
        foreach (var item in _barImage)
        {
            item.sprite = _barClose;
        }

        _barImage[_currentPage - 1].sprite = _barOpen;
    }

    public void MovePage(int page)
    {
        _currentPage = page;
        _targetPosition = _originalPosition + _pageStep * (page - 1);
        MovePage();
    }

    public void StartAutoScroll()
    {
        if (_autoScrollHandle.IsValid)
        {
            Timing.KillCoroutines(_autoScrollHandle);
        }

        _autoScrollHandle = Timing.RunCoroutine(AutoScroll());
    }

    public void StopAutoScroll()
    {
        if (_autoScrollHandle.IsValid)
        {
            Timing.KillCoroutines(_autoScrollHandle);
        }
    }

    private IEnumerator<float> AutoScroll()
    {
        while (_isAutoScrolling)
        {
            yield return Timing.WaitForSeconds(_autoScrollInterval);

            if (_currentPage < _maxPage)
            {
                NextPage();
            }
            else
            {
                MovePage(1);
            }
        }
    }
}
