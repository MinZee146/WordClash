using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILine : MonoBehaviour
{
    [SerializeField] private RectTransform _myTransform;
    [SerializeField] private Image _image;

    public void CreateLine(Vector3 positionOne, Vector3 positionTwo)
    {
        var point1 = (Vector2)positionTwo;
        var point2 = (Vector2)positionOne;
        var midpoint = (point1 + point2) / 2f;
        var dir = point1 - point2;

        _myTransform.position = midpoint;
        _myTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        _myTransform.localScale = new Vector3(dir.magnitude, 1f, 0f);
    }

    public void Validate()
    {
        _image.color = GameFlowManager.Instance.IsPlayerTurn || SceneManager.GetActiveScene().name == "TimeChallengeMode" ? Colors.FromHex(GameConstants.COLORS_PLAYER) : Colors.FromHex(GameConstants.COLORS_OPPONENT);
    }

    public void Invalidate()
    {
        _image.color = Colors.FromHex(GameConstants.COLORS_TILE_SELECT);
    }
}
