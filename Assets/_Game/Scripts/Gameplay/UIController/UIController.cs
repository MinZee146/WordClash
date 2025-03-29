using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : Singleton<GameUIController>
{
    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private RectTransform _boardRectTransform;

    private Coroutine _shakeHintCoroutine;
    private Coroutine _shakeConfirmCoroutine;

    private void OnEnable()
    {
        StartConfirmShakeRoutine();
    }

    private void OnDisable()
    {
        StopConfirmShakeRoutine();
    }
   
    public RectTransform ConfirmButtonRect()
    {
        return _confirmButton.GetComponent<RectTransform>();
    }

    public RectTransform BoardRectTransform()
    {
        return _boardRectTransform;
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
