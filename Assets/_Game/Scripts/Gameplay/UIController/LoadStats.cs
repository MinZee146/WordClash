using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LoadStats : Singleton<LoadStats>
{
    [SerializeField] private TextMeshProUGUI _name, _wins, _losses, _bestWord, _mostUsedPowerUp;
    [SerializeField] private Image _powerupImage;

    public void Load()
    {
        _name.text = $"{PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_USERNAME)}";
        _wins.text = $"{PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_WINS, 0)}";
        _losses.text = $"{PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_LOSSES, 0)}";
        _bestWord.text = $"{PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_BEST_WORD_OF_ALL_TIME, "No data")}";
        _mostUsedPowerUp.text = $"{System.Text.RegularExpressions.Regex.Replace(PlayerDataTracker.Instance.GetMostUsedPowerUp(), "(?<!^)([A-Z])", " $1")}";
        LoadPowerupImage(PlayerDataTracker.Instance.GetMostUsedPowerUp());
    }

    private void LoadPowerupImage(string powerupName)
    {
        var validKeys = new[] { "RevealWord", "TimeFreeze", "Grief", "ShortPenalty", "LongBonus", "ShortBonus", "DoubleScore", "Cleanse", "ExtraTurn", "ReplaceLetter", "Shuffle" };

        if (validKeys.Contains(powerupName))
        {
            Addressables.LoadAssetAsync<Sprite>(powerupName).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _powerupImage.sprite = handle.Result;
                }
            };
        }

    }
}
