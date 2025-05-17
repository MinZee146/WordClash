using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DemoTiles : Singleton<DemoTiles>
{
    [SerializeField] private GameObject _linePrefab, _cursor, _confirmButton;
    [SerializeField] private List<Tile> _tileList;

    private bool _isDragging;
    private bool _isClosing;

    private Vector3 _originalPos, _targetPos;
    private EventSystem _eventSystem;
    private GraphicRaycaster _graphicRaycaster;
    private List<Tile> _selectingTiles = new();
    private List<GameObject> _lineList = new();
    private Tween _cursorTween;
    private CoroutineHandle _shakeConfirmCoroutine;

    private void OnEnable()
    {
        _graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        _eventSystem = FindObjectOfType<EventSystem>();
        SetTemporaryCoordinates();
        StartConfirmShakeRoutine();
    }

    private void OnDisable()
    {
        StopConfirmShakeRoutine();
    }

    private void Update()
    {
        if (!_isClosing)
        {
            HandleInput();
        }
    }

    public void CursorAnimation()
    {
        _originalPos = _tileList[0].transform.position;
        _targetPos = _tileList[4].transform.position;

        AnimateCursor();
    }

    private void StartConfirmShakeRoutine()
    {
        StopConfirmShakeRoutine();
        _shakeConfirmCoroutine = Timing.RunCoroutine(ConfirmShakeAfterDelay());
    }

    private void StopConfirmShakeRoutine()
    {
        if (_shakeConfirmCoroutine != null)
        {
            Timing.KillCoroutines(_shakeConfirmCoroutine);
            _shakeConfirmCoroutine = default;
        }
    }

    private IEnumerator<float> ConfirmShakeAfterDelay()
    {
        yield return Timing.WaitForSeconds(1f);

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

    private void AnimateCursor()
    {
        if (_cursorTween != null && _cursorTween.IsActive())
        {
            _cursorTween.Kill();
            _cursorTween = null;
        }

        _cursor.SetActive(true);
        _cursor.transform.position = _originalPos;
        _cursorTween = _cursor.transform.DOMove(_targetPos, 1.5f).OnComplete(() =>
        {
            AnimateCursor();
        });
    }

    private void StopAnimation()
    {
        if (_cursorTween != null && _cursorTween.IsActive())
        {
            _cursorTween.Kill();
        }

        _cursor.transform.DOKill();
        _cursor.SetActive(false);
        _cursor.transform.position = _originalPos;
    }

    private void SetTemporaryCoordinates()
    {
        for (var i = 0; i < _tileList.Count; i++)
        {
            _tileList[i].Column = i;
            _tileList[i].Color = Color.white;
            _tileList[i].Deselect();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_confirmButton.activeSelf || !RectTransformUtility.RectangleContainsScreenPoint(_confirmButton.GetComponent<RectTransform>(), Input.mousePosition))
            {
                _confirmButton.SetActive(false);
                DeselectAll();
                DisconnectAll();
                StopAnimation();

                _selectingTiles.Clear();
                _isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;

            if (_selectingTiles.Count == 0)
            {
                AnimateCursor();
            }
        }

        if (_isDragging)
        {
            HandleDragging();
        }
    }

    private void HandleDragging()
    {
        var pointerEventData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        _graphicRaycaster.Raycast(pointerEventData, results);

        foreach (var tile in results.Select(result => result.gameObject.GetComponent<Tile>()).Where(tile => tile))
        {
            if (_selectingTiles.Count == 0)
            {
                SelectTile(tile);
            }
            else if (_selectingTiles[^1].IsAdjacent(tile))
            {
                HandleTileSelection(tile);
            }

            break;
        }
    }

    private void HandleTileSelection(Tile tile)
    {
        if (_selectingTiles.Contains(tile))
        {
            if (tile != _selectingTiles[^2]) return;

            _selectingTiles[^1].Deselect();
            _selectingTiles.Remove(_selectingTiles[^1]);

            DisconnectLastLine();
            AudioManager.Instance.PlaySFX("TileSelect");
        }
        else
        {
            tile.Select();
            Connect(tile);
            _selectingTiles.Add(tile);
        }

        if (_selectingTiles.Count == 5 && _selectingTiles[0] == _tileList[0])
        {
            _confirmButton.SetActive(true);

            foreach (var t in _selectingTiles)
            {
                t.ValidateWord();
            }

            foreach (var line in _lineList)
            {
                line.GetComponent<UILine>().Validate();
            }
        }
        else
        {
            _confirmButton.SetActive(false);

            foreach (var t in _selectingTiles)
            {
                t.InvalidateWord();
            }

            foreach (var line in _lineList)
            {
                line.GetComponent<UILine>().Invalidate();
            }
        }
    }

    public void Confirm()
    {
        _isClosing = true;

        AudioManager.Instance.PlaySFX("ButtonClick");
        PopUpsManager.Instance.ToggleInstructionPopUp(false);
    }

    private void SelectTile(Tile tile)
    {
        tile.Select();
        _selectingTiles.Add(tile);
    }

    public void DeselectAll()
    {
        foreach (var tile in _selectingTiles)
        {
            tile.Deselect();
        }
    }

    private void Connect(Tile tile)
    {
        if (_selectingTiles.Contains(tile)) return;

        var line = Instantiate(_linePrefab, transform);
        line.transform.SetSiblingIndex(0);
        line.GetComponent<UILine>().CreateLine(_selectingTiles[^1].transform.position, tile.transform.position);

        _lineList.Add(line);
    }

    private void DisconnectLastLine()
    {
        Destroy(_lineList[^1]);
        _lineList.Remove(_lineList[^1]);
    }

    public void DisconnectAll()
    {
        foreach (var line in _lineList)
        {
            Destroy(line.gameObject);
        }

        _lineList.Clear();
    }
}