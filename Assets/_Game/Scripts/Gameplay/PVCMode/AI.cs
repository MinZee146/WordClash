using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class AI : Singleton<AI>
{
    [NonSerialized] public string ForcedWord;
    [NonSerialized] public bool PreferLong, PreferShort;
    [SerializeField] private PowerUpBase[] _sidePowerUp;

    private bool _usedShuffle, _usedFreeze;

    public IEnumerator<float> AITurn()
    {
        yield return Timing.WaitForSeconds(Random.Range(2f, 5f));

        //AI use powerups
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(AIUseRandomSidePowerUp()));

        if (!PowerUpsManager.Instance.CheckExtraTurn)
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(ChoosePowerUp()));
        }

        yield return Timing.WaitForSeconds(Random.Range(1f, 2f));

        //AI select word
        var longWords = Board.Instance.FoundWords.Keys.Where(word => word.Length >= 5).ToList();
        var shortWords = Board.Instance.FoundWords.Keys.Where(word => word.Length < 5).ToList();

        var randomValue = Random.Range(0f, 1f);
        List<string> selectedList;

        if (PreferLong && longWords.Count > 0)
        {
            selectedList = longWords;
        }
        else if (PreferShort && shortWords.Count > 0)
        {
            selectedList = shortWords;
        }
        else if (randomValue <= RemoteConfigs.Instance.GameConfigs.AIDifficulty && longWords.Count > 0)
        {
            selectedList = longWords;
        }
        else
        {
            selectedList = shortWords;
        }

        var randomWord = string.IsNullOrEmpty(ForcedWord) ? selectedList[Random.Range(0, selectedList.Count)] : ForcedWord;

        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Board.Instance.OpponentSelect(randomWord)));
        yield return Timing.WaitForSeconds(0.75f);
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Board.Instance.PopAndRefresh()));

        ForcedWord = null;

        if (!GameManager.Instance.IsGameOver && PowerUpsManager.Instance.CheckExtraTurn)
        {
            yield return Timing.WaitForSeconds(Random.Range(1f, 2f));
            Timing.RunCoroutine(AITurn(), "AI");
        }
    }

    private IEnumerator<float> ChoosePowerUp()
    {
        if (PowerUpsManager.Instance.PowerUpCounts() == 0 || GameFlowManager.Instance.Turn <= 2)
        {
            yield break;
        }

        yield return Timing.WaitForSeconds(Random.Range(1f, 2f));

        var (description, image) = PowerUpsManager.Instance.AIUseRandomPowerUp();
        PopUpsManager.Instance.ToggleOpponentPowerupPopUp(true, description, image);
        yield return Timing.WaitForSeconds(2f);

        PopUpsManager.Instance.ToggleOpponentPowerupPopUp(false, description, image);
        yield return Timing.WaitForSeconds(0.5f);

        {
            if (PowerUpsManager.Instance.CheckRevealWord)
            {
                yield return Timing.WaitForSeconds(0.25f);
                PopUpsManager.Instance.ToggleRevealWordPopUp(true);
                yield return Timing.WaitForSeconds(2f);
                PopUpsManager.Instance.ToggleRevealWordPopUp(false);
                yield return Timing.WaitForSeconds(0.25f);
            }

            if (PowerUpsManager.Instance.CheckReplaceLetter)
            {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(AIReplaceTile()));
            }
        }
    }

    public void ResetPowerups()
    {
        _usedShuffle = false;
        _usedFreeze = false;
    }

    public IEnumerator<float> AIUseRandomSidePowerUp()
    {
        if (Random.Range(0f, 1f) > 0.25f || Board.Instance.FoundWords.Keys.Count <= 10)
        {
            yield break;
        }

        var selectedPowerUp = _sidePowerUp
            .Where(p => (p.GetName() == "Shuffle" && !_usedShuffle) ||
                        (p.GetName() == "TimeFreeze" && !_usedFreeze))
            .OrderBy(_ => Random.value)
            .FirstOrDefault();

        if (selectedPowerUp == null)
        {
            yield break;
        }

        AudioManager.Instance.PlaySFX("PowerupSelect");
        PopUpsManager.Instance.ToggleOpponentPowerupPopUp(true, selectedPowerUp.Description, selectedPowerUp.Sprite);

        if (selectedPowerUp.GetName() == "Shuffle")
        {
            _usedShuffle = true;
        }
        if (selectedPowerUp.GetName() == "TimeFreeze")
        {
            _usedFreeze = true;
        }

        Notifier.Instance.OnUsePowerUp(selectedPowerUp.GetName());
        yield return Timing.WaitForSeconds(1.5f);

        PopUpsManager.Instance.ToggleOpponentPowerupPopUp(false, selectedPowerUp.Description, selectedPowerUp.Sprite);
        yield return Timing.WaitForSeconds(0.5f);

        selectedPowerUp.ApplyPowerUp();
    }

    private IEnumerator<float> AIReplaceTile()
    {
        var incompleteWord = WordFinder.Instance.FindIncompleteWord();
        var lastChar = incompleteWord[^1];
        var currentWord = incompleteWord[..^1];

        var lastLetterPos = Board.Instance.FoundWords[currentWord].Path.Last();
        var lastLetterTile = Board.Instance.TileList.FirstOrDefault(t => t.Row == lastLetterPos.x && t.Column == lastLetterPos.y);
        var neighbors = Board.Instance.TileList.Where(lastLetterTile.IsAdjacent).ToList();

        var randomNeighbor = neighbors[Random.Range(0, neighbors.Count)];
        while (Board.Instance.FoundWords[currentWord].Path.Contains(new Vector2Int(randomNeighbor.Row, randomNeighbor.Column)))
        {
            randomNeighbor = neighbors[Random.Range(0, neighbors.Count)];
        }

        yield return Timing.WaitUntilDone(Timing.RunCoroutine(AIReplaceTile(lastChar, randomNeighbor)));

        Board.Instance.FoundWords[incompleteWord] = new FoundWordData(new List<Vector2Int>(Board.Instance.FoundWords[currentWord].Path) { new(randomNeighbor.Row, randomNeighbor.Column) }, 0);
        ForcedWord = incompleteWord;
    }

    private IEnumerator<float> AIReplaceTile(char lastChar, Tile randomNeighbor)
    {
        yield return Timing.WaitForSeconds(0.25f);
        randomNeighbor.Select();
        yield return Timing.WaitForSeconds(0.5f);
        randomNeighbor.Deselect();
        yield return Timing.WaitForSeconds(0.5f);

        PopUpsManager.Instance.ToggleLetterReplacePopUp(true, (letter) =>
        {
            Timing.RunCoroutine(Replace(letter, lastChar));
        });

        yield return Timing.WaitForSeconds(2.5f);
        PopUpsManager.Instance.ToggleLetterReplacePopUp(false);
        yield return Timing.WaitForSeconds(1f);

        randomNeighbor.transform.DOScale(Vector3.one * 0.1f, 0.3f).OnComplete(() =>
         {
             randomNeighbor.SetTileConfig(Board.Instance.GetConfig(lastChar));
             randomNeighbor.transform.DOScale(Vector3.one, 0.3f);
         });
    }

    private IEnumerator<float> Replace(TMP_InputField letter, char lastChar)
    {
        yield return Timing.WaitForSeconds(1.5f);
        letter.text = lastChar.ToString();
    }

    public int GetUnusedPowerupsCount()
    {
        return (_usedFreeze ? 0 : 1) + (_usedShuffle ? 0 : 1);
    }
}
