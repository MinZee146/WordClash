using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopUps : MonoBehaviour
{
    [SerializeField] private float _delayTime;
    [SerializeField] private GameObject _effectGroup;

    private string key;

    public void SetKey(string tag)
    {
        key = tag;
    }

    private void OnEnable()
    {
        var panel = GetComponent<Image>();
        var rectTransform = _effectGroup.GetComponent<RectTransform>();

        var panelColor = panel.color;
        panelColor.a = 90f / 255f;
        panel.color = panelColor;

        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            DOVirtual.DelayedCall(_delayTime, () =>
            {
                rectTransform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
                {
                    PopUpsPool.Instance.ReturnToPool(key, gameObject);
                });
            });
        });
    }
}
