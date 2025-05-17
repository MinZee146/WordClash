using UnityEngine;

public class HomeUIController : SingletonPersistent<HomeUIController>
{
    [SerializeField] private GameObject _navBar, _homeScreen, _shopScreen;

    public CanvasGroup HomeCanvasGroup;

    public void ToggleHomeUI(bool setActive)
    {
        _navBar.SetActive(setActive);
        _homeScreen.SetActive(setActive);
    }

    public void PlayWithComputer()
    {
        UIManager.Instance.LoadGameScene();
    }

    public void OpenSettings()
    {
        PopUpsManager.Instance.ToggleSettingsPopUp(true);
    }

    public void OpenThemeSelector()
    {
        PopUpsManager.Instance.ToggleThemePopUp(true);
    }

    public void OpenStats()
    {
        PopUpsManager.Instance.ToggleStatsPopUp(true);
    }

    public void OpenDailyReward()
    {
        PopUpsManager.Instance.ToggleDailyRewardPopUp(true);
    }
}
