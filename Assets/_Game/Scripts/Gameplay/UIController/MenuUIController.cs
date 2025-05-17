using DG.Tweening;
using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    [SerializeField] private float _titleJiggleDistance = 5f;
    [SerializeField] private GameObject _title;
    private RectTransform _titleRectTransform;

    private void Start()
    {
        _titleRectTransform = _title.GetComponent<RectTransform>();

        StartJiggleAnimation();
    }

    private void StartJiggleAnimation()
    {
        var originalY = _titleRectTransform.position.y;

        _titleRectTransform
            .DOMoveY(originalY + _titleJiggleDistance, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
