using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image _innerCircle;
    [SerializeField] private TextMeshProUGUI _scoreText, _letterText;
    [SerializeField] private Gradient _gradient;

    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsRowEven { get; set; }
    public char Letter { get; private set; }
    public int Score { get; private set; }
    public Color Color { get; set; }

    private Color _selectedTileColor = Colors.FromHex("838080"),
                 _validatedTileColor = Colors.FromHex("6EC207"),
                _darkText = Colors.FromHex("181C14"),
                _lightText = Colors.FromHex("FAF7F0"),
                _opponentValidColor = Colors.FromHex("ED254E");

    public bool IsAdjacent(Tile tile)
    {
        if (IsRowEven)
        {
            return (tile.Column == Column - 1 && tile.Row >= Row - 1 && tile.Row <= Row + 1) ||
                   (tile.Column == Column && (tile.Row == Row - 1 || tile.Row == Row + 1)) ||
                   (tile.Column == Column + 1 && tile.Row == Row);
        }
        return (tile.Column == Column - 1 && tile.Row == Row) ||
               (tile.Column == Column && (tile.Row == Row - 1 || tile.Row == Row + 1)) ||
               (tile.Column == Column + 1 && tile.Row >= Row - 1 && tile.Row <= Row + 1);
    }

    public void SetTileConfig(TileConfig config)
    {
        //Get config
        Letter = config.Letter;
        Score = config.Score;
        Color = config.Color;

        //Apply config
        _scoreText.text = Score.ToString();
        _letterText.text = Letter.ToString();
        _innerCircle.color = Color;
    }

    public void Select()
    {
        _innerCircle.color = _selectedTileColor;
        _letterText.color = _lightText;
        _scoreText.color = _lightText;

        _innerCircle.transform.DOComplete();
        _innerCircle.transform.DOShakePosition(0.5f, 10f, 10, 75, false, true, ShakeRandomnessMode.Harmonic);

        // AudioManager.Instance.PlaySFX("TileSelect");
    }

    public void Deselect()
    {
        _innerCircle.color = Color;
        _letterText.color = _darkText;
        _scoreText.color = _darkText;

        _innerCircle.transform.DOComplete();
        _innerCircle.transform.DOShakePosition(0.5f, 10f, 10, 75, false, true, ShakeRandomnessMode.Harmonic);
    }

    public void ValidateWord()
    {
        _innerCircle.color = _validatedTileColor;
        _letterText.color = _scoreText.color = _lightText;
    }

    public void InvalidateWord()
    {
        _innerCircle.color = _selectedTileColor;
        _letterText.color = _lightText;
        _scoreText.color = _lightText;
    }

    public Tween Hint(bool isLastChar)
    {
        _letterText.color = _lightText;
        _scoreText.color = _lightText;

        return _innerCircle.DOGradientColor(_gradient, 0.4f).OnComplete(() =>
        {
            Deselect();

            if (!isLastChar)
            {
                // AudioManager.Instance.PlaySFX("Hint");
            }
        });
    }

    public IEnumerator<float> PopAndDestroy()
    {
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(PopAnimation()));

        var random = Random.Range(0f, 1f);

        if (random > 0.5f)
        {
            // AudioManager.Instance.PlaySFX("Pop1");
        }
        else
        {
            // AudioManager.Instance.PlaySFX("Pop2");
        }

        Destroy(gameObject);
    }

    IEnumerator<float> PopAnimation()
    {
        transform.DOScale(0f, 0.5f);

        yield return Timing.WaitForSeconds(0.2f);
    }
}

