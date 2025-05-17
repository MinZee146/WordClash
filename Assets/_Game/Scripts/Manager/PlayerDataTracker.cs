using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerDataTracker : SingletonPersistent<PlayerDataTracker>
{
    private string _bestWordOfAllTime;
    private int _bestWordScore;
    private int _wins, _losses;
    private Dictionary<string, int> _powerUpUsage;

    public void Initialize()
    {
        _bestWordOfAllTime = PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_BEST_WORD_OF_ALL_TIME, "");
        _bestWordScore = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_BEST_WORD_SCORE, 0);
        _wins = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_WINS, 0);
        _losses = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_LOSSES, 0);

        if (PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_POWERUP_USAGE))
        {
            var jsonData = PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_POWERUP_USAGE);
            var data = JsonUtility.FromJson<SerializableDictionary>(jsonData);
            _powerUpUsage = data.ToDictionary();
        }
        else
        {
            _powerUpUsage = new Dictionary<string, int>();
        }
    }

    public void LogPowerUpUsage(string powerupName)
    {
        if (_powerUpUsage.ContainsKey(powerupName))
        {
            _powerUpUsage[powerupName]++;
        }
        else
        {
            _powerUpUsage[powerupName] = 1;
        }

        SavePowerUpUsage();
    }

    public string GetMostUsedPowerUp()
    {
        if (_powerUpUsage.Count == 0) return "No data";
        var mostUsed = _powerUpUsage.Aggregate((x, y) => x.Value > y.Value ? x : y);
        return mostUsed.Key;
    }

    public void LogBestWord(string word, int score)
    {
        if (score > _bestWordScore)
        {
            _bestWordOfAllTime = word;
            PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_BEST_WORD_OF_ALL_TIME, _bestWordOfAllTime);
        }
    }

    public void LogBattleResult(bool isPlayerWon, string word, int score)
    {
        if (isPlayerWon)
        {
            _wins++;
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_WINS, _wins);
        }
        else
        {
            _losses++;
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_LOSSES, _losses);
        }

        LogBestWord(word, score);
    }

    private void SavePowerUpUsage()
    {
        var jsonData = JsonUtility.ToJson(new SerializableDictionary(_powerUpUsage));
        PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_POWERUP_USAGE, jsonData);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class SerializableDictionary
    {
        public List<string> keys = new List<string>();
        public List<int> values = new List<int>();

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<string, int> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<string, int> ToDictionary()
        {
            var result = new Dictionary<string, int>();
            for (var i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i];
            }
            return result;
        }
    }
}
