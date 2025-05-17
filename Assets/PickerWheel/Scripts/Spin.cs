using DG.Tweening;
using EasyUI.PickerWheelUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Spin : MonoBehaviour
{
    [SerializeField] private Button _spinButton, _closeButton;
    [SerializeField] private TextMeshProUGUI _spinText;
    [SerializeField] private PickerWheel _wheel;

    private void Start()
    {
        var hasSpunToday = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_HAS_SPUN_TODAY, 0) == 1;
        _spinButton.interactable = !hasSpunToday;

        if (hasSpunToday)
        {
            _spinText.text = "Claimed";
        }
        else
        {
            _spinText.text = "Spin";
        }

        _spinButton.onClick.AddListener(() =>
        {
            _closeButton.gameObject.SetActive(false);
            _spinButton.interactable = false;
            _spinText.text = "Spinning";

            _wheel.OnSpinEnd((piece) =>
            {
                _closeButton.gameObject.SetActive(true);
                _spinText.text = "Claimed";

                AudioManager.Instance.PlaySFX("Bell");
                RewardManager.Instance.DisableSpin();

                _spinButton.interactable = false;
                _spinText.text = "Claimed";

                switch (piece.Label)
                {
                    case "Coins":
                        CurrencyManager.Instance.CoinsAttract(piece.Amount, transform.position);
                        break;
                    case "Hints":
                        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER,
                        PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_HINT_COUNTER) ?
                        PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_HINT_COUNTER) + piece.Amount :
                        RemoteConfigs.Instance.GameConfigs.InitialHints + piece.Amount);
                        break;
                }
            });

            _wheel.Spin();
        });
    }
}
