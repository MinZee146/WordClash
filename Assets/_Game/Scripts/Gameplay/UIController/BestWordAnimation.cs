using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BestWordAnimation : MonoBehaviour
{
    [SerializeField] private GameObject _title;
    [SerializeField] private TextMeshProUGUI _text;

    private AsyncOperationHandle _handle;

    public void SetProps(string word, AsyncOperationHandle handle)
    {
        _text.text = word;
        _handle = handle;
    }

    private void OnEnable()
    {
        _title.transform.localScale = Vector3.zero;
        _title.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);

        WobbleAndRainbow();
    }

    private void WobbleAndRainbow()
    {
        var wobbleDuration = 0.6f;
        var changeColorDuration = 0.5f;
        var defaultTextColor = new Color(60 / 255f, 0, 50 / 255f);
        var textInfo = _text.textInfo;

        _text.ForceMeshUpdate();

        for (var i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            var vertexIndex = charInfo.vertexIndex;
            var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            var originalVertice = new Vector3[4];
            originalVertice[0] = vertices[vertexIndex];
            originalVertice[1] = vertices[vertexIndex + 1];
            originalVertice[2] = vertices[vertexIndex + 2];
            originalVertice[3] = vertices[vertexIndex + 3];

            var materialIndex = charInfo.materialReferenceIndex;
            var vertexColors = textInfo.meshInfo[materialIndex].colors32;

            var delay = i * 0.1f;

            DOTween.To(() => 0f, value =>
            {
                var wobbleAmount = Mathf.Sin(value * Mathf.PI * 2f) * 10f;

                for (var j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalVertice[j] + new Vector3(0, wobbleAmount, 0);
                }

                _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                var color = Color.HSVToRGB((value + i * 0.1f) % 1f, 1f, 1f);
                
                for (var j = 0; j < 4; j++)
                {
                    vertexColors[vertexIndex + j] = color;
                }

                _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            }, 1f, wobbleDuration)
            .SetEase(Ease.InOutSine)
            .SetDelay(delay)
            .OnComplete(() =>
            {
                DOTween.To(() => vertexColors[vertexIndex], color =>
                {
                    for (var j = 0; j < 4; j++)
                    {
                        vertexColors[vertexIndex + j] = color;
                    }

                    _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }, defaultTextColor, changeColorDuration).SetEase(Ease.InOutSine);
            });
        }
    }

    public void CleanUp()
    {
        Addressables.Release(_handle);
        Destroy(gameObject);
    }
}
