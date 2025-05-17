using System;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;

public class GameOverAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform _coinsRewardRect, _playerStatsRect, _opponentStatsRect, _winTrophyRect, _loseTrophyRect, _coinIconRect;
    [SerializeField] private TextMeshProUGUI _playerBestWord, _opponentBestWord, _coinsEarnedText;

    private Vector3 _playerPositon = new(-150f, 205f, 0f);
    private Vector3 _opponentPositon = new(25f, 25f, 0f);

    private void OnEnable()
    {
        Reset();

        var sequence = DOTween.Sequence();
        var showPlayerStats = _playerStatsRect.DOScale(Vector3.one, 0.75f).SetEase(Ease.OutBounce);
        var showOpponentStats = _opponentStatsRect.DOScale(Vector3.one, 0.75f).SetEase(Ease.OutBounce);

        sequence.Append(showPlayerStats);
        sequence.AppendInterval(0.25f);
        sequence.Append(showOpponentStats);
        sequence.AppendInterval(0.5f);

        var showWinTrophy = _winTrophyRect.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
        var showLoseTrophy = _loseTrophyRect.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);

        showWinTrophy.onPlay += () =>
        {
            AudioManager.Instance.PlaySFX("PowerupSelect");
        };
        showLoseTrophy.onPlay += () =>
        {
            AudioManager.Instance.PlaySFX("PowerupSelect");
        };

        sequence.Append(showWinTrophy);
        sequence.Append(showLoseTrophy);
        sequence.AppendInterval(0.5f);

        var bestWord = PlayerStatsManager.Instance.IsPlayerHavingBestWord() ? _playerBestWord : _opponentBestWord;
        sequence.onComplete += () =>
        {
            WobbleAndRainbow(bestWord);
            CoinsEarnedAnimation();
            AudioManager.Instance.PlayMusic(PlayerStatsManager.Instance.IsPlayerWon() ? "Win" : "Lose");
        };
    }

    private void Reset()
    {
        _playerStatsRect.localScale = Vector3.zero;
        _opponentStatsRect.localScale = Vector3.zero;
        _coinsRewardRect.localScale = Vector3.zero;
        _coinsEarnedText.text = "";

        _winTrophyRect.localScale = Vector3.zero;
        _loseTrophyRect.localScale = Vector3.zero;
        _winTrophyRect.localPosition = PlayerStatsManager.Instance.IsPlayerWon() ? _playerPositon : _opponentPositon;
        _loseTrophyRect.localPosition = PlayerStatsManager.Instance.IsPlayerWon() ? _opponentPositon : _playerPositon;
    }

    private void CoinsEarnedAnimation()
    {
        var coins = PlayerStatsManager.Instance.IsPlayerWon()
            ? RemoteConfigs.Instance.GameConfigs.CoinsPerGame + PlayerStatsManager.Instance.ScoreGap() / 5f
            : RemoteConfigs.Instance.GameConfigs.CoinsPerGame;

        var roundedCoins = (int)Math.Round(coins);

        var sequence = DOTween.Sequence();
        sequence.Append(_coinsRewardRect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce));
        sequence.AppendInterval(0.25f);
        sequence.onComplete += () =>
        {
            Timing.RunCoroutine(AnimateCoinIncrease(roundedCoins));
        };
    }

    private IEnumerator<float> AnimateCoinIncrease(int increment)
    {
        var startCoins = 0;
        var targetCoins = increment;

        var duration = 0.5f;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Timing.DeltaTime;
            var progress = elapsedTime / duration;
            var newCoins = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, progress));
            _coinsEarnedText.text = newCoins.ToString();

            yield return Timing.WaitForOneFrame;
        }

        _coinsEarnedText.text = targetCoins.ToString();

        CurrencyManager.Instance.CoinsAttract(Convert.ToInt32(_coinsEarnedText.text), _coinIconRect.position);
    }

    private void WobbleAndRainbow(TextMeshProUGUI text)
    {
        var wobbleDuration = 0.6f;
        var changeColorDuration = 0.5f;
        var defaultTextColor = new Color(255 / 255f, 255 / 255f, 255 / 255f);
        var textInfo = text.textInfo;

        text.ForceMeshUpdate(true, true);

        var fullText = text.text;
        var colonIndex = fullText.IndexOf(":");
        if (colonIndex == -1) return;

        var startCharIndex = colonIndex + 1;

        for (var i = startCharIndex; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
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
            var delay = (i - startCharIndex) * 0.1f; // Offset delay based on index

            DOTween.To(() => 0f, value =>
            {
                var wobbleAmount = Mathf.Sin(value * Mathf.PI * 2f) * 10f;

                for (var j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalVertice[j] + new Vector3(0, wobbleAmount, 0);
                }

                text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                var color = Color.HSVToRGB((value + (i - startCharIndex) * 0.1f) % 1f, 1f, 1f);

                for (var j = 0; j < 4; j++)
                {
                    vertexColors[vertexIndex + j] = color;
                }

                text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

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

                    text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }, defaultTextColor, changeColorDuration).SetEase(Ease.InOutSine);
            });
        }
    }
}
