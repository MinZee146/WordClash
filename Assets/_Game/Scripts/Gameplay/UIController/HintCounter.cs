using TMPro;
using UnityEngine;

public class HintCounter : Singleton<HintCounter>
{
    public int CurrentHintCounter => _currentHintCounter;

    [SerializeField] private TextMeshProUGUI _hintText;

    private int _currentHintCounter;

    public void FetchHintPref()
    {
        _currentHintCounter = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER, RemoteConfigs.Instance.GameConfigs.InitialHints);
        _hintText.text = _currentHintCounter != 0 ? _currentHintCounter.ToString() : "+";
    }

    public void UpdateCounter()
    {
        _currentHintCounter--;
        _hintText.text = _currentHintCounter != 0 ? _currentHintCounter.ToString() : "+";

        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER, _currentHintCounter);
    }
}
