using TMPro;
using UnityEngine;

public class NameRegister : SingletonPersistent<NameRegister>
{
    public bool Registered = true;

    public void Initialize()
    {
        CheckHaveRegister();
    }

    private void CheckHaveRegister()
    {
        if (!PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_USERNAME))
        {
            PopUpsManager.Instance.ToggleNameRegisterPopUp(true);
            Registered = false;
        }
        else if (!PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_CURRENT_AVATAR))
        {
            PopUpsManager.Instance.ToggleAvatarPopUp(true);
        }
    }

    public void ConfirmUsername(TMP_InputField usernameInput)
    {
        if (!string.IsNullOrEmpty(usernameInput.text))
        {
            PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_USERNAME, usernameInput.text);
            PlayerPrefs.Save();

            Registered = true;

            PopUpsManager.Instance.ToggleNameRegisterPopUp(false, true);
        }
    }

    public void ChangeNameAndAvatar()
    {
        PopUpsManager.Instance.ToggleNameRegisterPopUp(true);
        PopUpsManager.Instance.ToggleStatsPopUp(false);
    }
}
