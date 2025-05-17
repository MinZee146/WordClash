using UnityEngine;

public class AvatarSelector : MonoBehaviour
{
    public void SelectAvatar(string key)
    {
        PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_CURRENT_AVATAR, key);
    }
}
