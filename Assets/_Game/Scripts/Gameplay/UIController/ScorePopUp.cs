using System;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScorePopUp : Singleton<ScorePopUp>
{
    [SerializeField] private Sprite _playerStar, _opponentStar;
    [SerializeField] private GameObject _playerScoreBoard, _opponentScoreBoard;
    [SerializeField] private ParticleImage _particleScore;

    private bool _isAnimating;

    public IEnumerator<float> ScoreAttract(int rate, Transform startPosition, Action action)
    {
        var starSprite = GameFlowManager.Instance.IsPlayerTurn || SceneManager.GetActiveScene().name == "TimeChallengeMode" ? _playerStar : _opponentStar;
        var color = GameFlowManager.Instance.IsPlayerTurn ? Colors.FromHex("FFCC0080") : Colors.FromHex("FF555580");
        var endPositon = GameFlowManager.Instance.IsPlayerTurn || SceneManager.GetActiveScene().name == "TimeChallengeMode" ? _playerScoreBoard.transform : _opponentScoreBoard.transform;

        _particleScore.sprite = starSprite;
        _particleScore.trailColorOverLifetime = color;
        _particleScore.attractorTarget = endPositon;
        _particleScore.emitterConstraintTransform = startPosition;

        _particleScore.onParticleStarted.RemoveAllListeners();
        _particleScore.onParticleStop.RemoveAllListeners();
        _particleScore.onFirstParticleFinished.RemoveAllListeners();
        _particleScore.onAnyParticleFinished.RemoveAllListeners();

        for (var i = 0; i < rate; i++)
        {
            _particleScore.AddBurst(i / 7.5f, 1);
        }

        _particleScore.onParticleStarted.AddListener(() =>
       {
           _isAnimating = true;
       });

        _particleScore.onParticleStop.AddListener(() =>
        {
            _isAnimating = false;

            for (var i = 1; i <= rate; i++)
            {
                _particleScore.RemoveBurst(rate - i);
            }
        });

        _particleScore.onFirstParticleFinished.AddListener(() =>
        {
            action.Invoke();
        });

        _particleScore.onAnyParticleFinished.AddListener(() =>
        {
            switch (rate - _particleScore.particleCount + 1)
            {
                case 1:
                case 2:
                    AudioManager.Instance.PlaySFX("Score1");
                    break;
                case 3:
                case 4:
                    AudioManager.Instance.PlaySFX("Score2");
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    AudioManager.Instance.PlaySFX("Score3");
                    break;
            }
        });

        _particleScore.Play();

        while (_isAnimating)
        {
            yield return 0f;
        }

        yield return Timing.WaitForSeconds(0.25f);
    }
}
