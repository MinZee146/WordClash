using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WordDisplay : Singleton<WordDisplay>
{
    [SerializeField] private TextMeshProUGUI _wordDisplayText;
    [SerializeField] private RectTransform _wordDisplayRect;

    public void UndisplayWordAndScore()
    {
        _wordDisplayText.text = string.Empty;
        _wordDisplayRect.sizeDelta = new Vector2(0, 0);
    }

    private void Validate(List<GameObject> lineList, List<Tile> selectingTiles)
    {
        foreach (var tile in selectingTiles)
        {
            tile.ValidateWord();
        }

        foreach (var line in lineList)
        {
            line.GetComponent<UILine>().Validate();
        }
    }

    private void Invalidate(List<GameObject> lineList, List<Tile> selectingTiles)
    {
        foreach (var tile in selectingTiles)
        {
            tile.InvalidateWord();
        }

        foreach (var line in lineList)
        {
            line.GetComponent<UILine>().Invalidate();
        }
    }

    public void UpdateWordState(Tile tile, string currentWord, ref int currentScore, List<GameObject> lineList, List<Tile> selectingTiles)
    {
        if (currentWord.Length > 1)
        {
            if (GameDictionary.Instance.CheckWord(currentWord))
            {
                _wordDisplayText.text = $"{currentWord} <color={UpdateScore(ref currentScore, selectingTiles)}>({currentScore})</color>";
                Validate(lineList, selectingTiles);
            }
            else
            {
                _wordDisplayText.text = currentWord;
                Invalidate(lineList, selectingTiles);
            }

            UpdateWordDisplayPosition(tile);
        }
        else
        {
            UndisplayWordAndScore();
            Invalidate(lineList, selectingTiles);
        }
    }

    private string UpdateScore(ref int currentScore, List<Tile> selectingTiles)
    {
        currentScore = selectingTiles.Count == 0 ? 0 : selectingTiles.Sum(tile => tile.Score) * selectingTiles.Count;
       
        var scoreColor = currentScore switch
        {
            <= 10 => "#EEEEEE",
            <= 20 => "#FFFF99",
            <= 30 => "#FFFF33",
            <= 40 => "#FFCC00",
            <= 50 => "#FF9900",
            <= 65 => "#FF5555",
            <= 80 => "#66FF00",
            _ => "#00FFFF",
        };

        return scoreColor;
    }

    private void UpdateWordDisplayPosition(Tile currentTile)
    {
        var paddingX = 55f;
        var paddingY = 90f;

        var tileRectTransform = currentTile.GetComponent<RectTransform>();
        _wordDisplayRect.position = tileRectTransform.position + new Vector3(0f, 150f);

        var panelWidth = _wordDisplayText.preferredWidth;
        _wordDisplayRect.sizeDelta = new Vector2(panelWidth + paddingX, paddingY);

        var canvas = _wordDisplayRect.GetComponentInParent<Canvas>();
        var canvasRect = canvas.GetComponent<RectTransform>();
        var canvasWidth = canvasRect.rect.width;
        var canvasHeight = canvasRect.rect.height;
        var canvasScaleFactor = canvas.scaleFactor;

        var maxX = (canvasWidth / 2) - _wordDisplayRect.sizeDelta.x / 2 / canvasScaleFactor - paddingX;
        var minX = -(canvasWidth / 2) + _wordDisplayRect.sizeDelta.x / 2 / canvasScaleFactor + paddingX;
        var maxY = (canvasHeight / 2) - _wordDisplayRect.sizeDelta.y / 2 / canvasScaleFactor - paddingX;
        var minY = -(canvasHeight / 2) + _wordDisplayRect.sizeDelta.y / 2 / canvasScaleFactor + paddingX;
        var clampedPosition = _wordDisplayRect.anchoredPosition;

        if (panelWidth + paddingX > canvasWidth)
        {
            while (panelWidth + paddingX > canvasWidth && _wordDisplayText.fontSize > 10)
            {
                _wordDisplayText.fontSize -= 1;
                panelWidth = _wordDisplayText.preferredWidth;
                _wordDisplayRect.sizeDelta = new Vector2(panelWidth + paddingX, paddingY);
            }

            clampedPosition = new Vector2(0, clampedPosition.y);
        }
        else
        {
            while (panelWidth + paddingX <= canvasWidth && _wordDisplayText.fontSize < 50)
            {
                _wordDisplayText.fontSize += 1;
                panelWidth = _wordDisplayText.preferredWidth;
                _wordDisplayRect.sizeDelta = new Vector2(panelWidth + paddingX, paddingY);
            }

            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        }

        _wordDisplayRect.anchoredPosition = clampedPosition;
    }
}
