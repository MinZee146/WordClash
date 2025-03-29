using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MEC;

public class PowerUpsManager : SingletonPersistent<PowerUpsManager>
{
    public bool CheckExtraTurn => _isExtraTurn;
    public bool CheckReplaceLetter => _isReplaceLetter;
    public bool CheckRevealWord => _isRevealWord;

    private bool _isBeingGrief, _isPenalty, _isCleansing, _isExtraTurn, _isReplaceLetter, _isRevealWord;
    private PowerUpBase _currentPowerUp;
    private PowerUpBase[] _powerUpsList = new PowerUpBase[6];
    private bool[] _powerUpsState = Enumerable.Repeat(true, 6).ToArray();
    private AsyncOperationHandle<IList<PowerUpBase>> _loadedPowerUpHandle;

    public int PowerUpCounts()
    {
        return _powerUpsState.Count(state => state);
    }

    public void Initialize()
    {
        UnloadPowerUps();
        CleanPowerUp();

        _loadedPowerUpHandle = Addressables.LoadAssetsAsync<PowerUpBase>("PowerUp", null);
        _loadedPowerUpHandle.Completed += OnPowerUpsLoaded;

        BottomBar.Instance.Reset();
        AI.Instance.ResetPowerups();
    }

    private void OnPowerUpsLoaded(AsyncOperationHandle<IList<PowerUpBase>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var powerUps = handle.Result
           .Where(x => x.GetName() != "TimeFreeze" && x.GetName() != "Shuffle")
           .OrderBy(x => Random.value)
           .ToList();

            for (var i = 0; i < 6; i++)
            {
                var index = i;
                _powerUpsList[index] = powerUps[index];
            }
        }
        else
        {
            Utils.LogError("Failed to load power-ups from Addressables.");
        }
    }

    public void UpdateButtons(List<Button> powerupList)
    {
        for (var i = 0; i < 6; i++)
        {
            SetPowerUpButtonState(powerupList[i], _powerUpsState[i]);
            var index = i;
            powerupList[i].onClick.AddListener(() => Timing.RunCoroutine(UsePowerUp(index)));
            powerupList[i].GetComponentInChildren<TextMeshProUGUI>().text = _powerUpsList[i].Description;
            powerupList[i].transform.GetChild(1).GetComponent<Image>().sprite = _powerUpsList[i].Sprite;
        }
    }

    private IEnumerator<float> UsePowerUp(int index)
    {
        AudioManager.Instance.PlaySFX("PowerupSelect");
        _powerUpsState[index] = false;

        if (GameFlowManager.Instance.IsPlayerTurn)
        {
            // PowerUpCountdown.Instance.Reset();
            PopUpsManager.Instance.TogglePowerupsPopUp(false);
            UIManager.Instance.IsInteractable = false;

            yield return Timing.WaitForSeconds(0.5f);
            UIManager.Instance.IsInteractable = true;
        }

        _currentPowerUp = _powerUpsList[index];
        _currentPowerUp.ApplyPowerUp();
        Notifier.Instance.OnUsePowerUp(_currentPowerUp.GetName());

        if (GameFlowManager.Instance.IsPlayerTurn)
        {
            PlayerDataTracker.Instance.LogPowerUpUsage(_currentPowerUp.GetName());
        }

        CheckForPowerUpAction();

        Utils.Log("Selected PowerUp: " + _powerUpsList[index].name);
    }

    public void PlayerUseRandomPowerUp()
    {
        var availableButtons = _powerUpsList
     .Select((powerup, index) => new { powerup, index })
     .Where(x => x.powerup != null && _powerUpsState[x.index])
     .Select(x => _powerUpsList[x.index])
     .ToList();

        var randomButton = availableButtons[Random.Range(0, availableButtons.Count)];
        var selectedPowerUpIndex = System.Array.IndexOf(_powerUpsList, randomButton);

        Timing.RunCoroutine(UsePowerUp(selectedPowerUpIndex));
    }

    public (string description, Sprite image) AIUseRandomPowerUp()
    {
        if (_isBeingGrief || _isPenalty)
        {
            for (var i = 0; i < _powerUpsList.Length; i++)
            {
                if (_powerUpsList[i].GetName() == "Cleanse" && _powerUpsState[i])
                {
                    Timing.RunCoroutine(UsePowerUp(i));
                    return (_powerUpsList[i].Description, _powerUpsList[i].Sprite);
                }
            }
        }

        var availableButtons = _powerUpsList
     .Select((powerup, index) => new { powerup, index })
     .Where(x => x.powerup != null && _powerUpsState[x.index])
     .Select(x => _powerUpsList[x.index])
     .ToList();
        var randomIndex = Random.Range(0, availableButtons.Count);
        var selectedPowerUpIndex = System.Array.IndexOf(_powerUpsList, availableButtons[randomIndex]);

        Timing.RunCoroutine(UsePowerUp(selectedPowerUpIndex));
        return (_powerUpsList[selectedPowerUpIndex].Description, _powerUpsList[selectedPowerUpIndex].Sprite);
    }

    public void CheckForPowerUpAction()
    {
        if (_currentPowerUp == null) return;

        switch (_currentPowerUp.GetName())
        {
            case "RevealWord":
                _isRevealWord = true;
                break;
            case "ReplaceLetter":
                _isReplaceLetter = true;
                break;
            case "ExtraTurn":
                _isExtraTurn = true;
                break;
        }
    }

    public void CheckForPowerUpScoring(ref int currentScore, int currentLength)
    {
        if (_isBeingGrief && _currentPowerUp == null)
        {
            currentScore /= 2;
        }

        if (_isPenalty && _currentPowerUp == null && currentLength < 5)
        {
            currentScore /= 2;
        }

        if (_currentPowerUp == null) return;

        switch (_currentPowerUp.GetName())
        {
            case "DoubleScore":
                currentScore *= 2;
                break;
            case "LongBonus":
                currentScore = currentLength >= 5 ? currentScore * 2 : currentScore;
                break;
            case "ShortBonus":
                currentScore = currentLength < 5 ? currentScore * 2 : currentScore;
                break;
            case "Grief":
                _isBeingGrief = true;
                break;
            case "ShortPenalty":
                _isPenalty = true;
                break;
            case "Cleanse":
                _isCleansing = true;
                break;
        }

        if (_isCleansing)
        {
            if (!_isBeingGrief && !_isPenalty)
            {
                currentScore = (int)(currentScore * 1.5f);
                return;
            }
        }
        else
        {
            if (_isBeingGrief && _currentPowerUp.GetName() != "Grief")
            {
                currentScore /= 2;
                return;
            }

            if (_isPenalty && _currentPowerUp.GetName() != "ShortPenalty" && currentLength < 5)
            {
                currentScore /= 2;
            }
        }
    }

    public void CleanPowerUp()
    {
        _isReplaceLetter = _isRevealWord = _isCleansing = false;

        if (_currentPowerUp == null)
        {
            _isExtraTurn = _isBeingGrief = _isPenalty = false;
            return;
        }

        if (_currentPowerUp.GetName() != "ExtraTurn")
        {
            _isExtraTurn = false;
        }

        if (_currentPowerUp.GetName() != "Grief")
        {
            _isBeingGrief = false;
        }

        if (_currentPowerUp.GetName() != "ShortPenalty")
        {
            _isPenalty = false;
        }

        _currentPowerUp = null;
        AI.Instance.PreferLong = AI.Instance.PreferShort = false;


    }

    public void UnloadPowerUps()
    {
        if (_loadedPowerUpHandle.IsValid())
        {
            Addressables.Release(_loadedPowerUpHandle);

            _powerUpsList = new PowerUpBase[6];
            _powerUpsState = Enumerable.Repeat(true, 6).ToArray();
        }
    }

    public void SetPowerUpButtonState(Button btn, bool state)
    {
        btn.interactable = state;

        foreach (Transform child in btn.transform)
        {
            if (child.GetComponent<Image>())
            {
                child.GetComponent<Image>().color = state ? Color.white : Colors.FromHex("7D7D7D");
            }

            if (child.GetComponent<TextMeshProUGUI>())
            {
                child.GetComponent<TextMeshProUGUI>().color = state ? Color.white : Colors.FromHex("7D7D7D");
            }
        }
    }
}
