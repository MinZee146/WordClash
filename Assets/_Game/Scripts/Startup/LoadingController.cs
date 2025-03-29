using System.Collections;
using UnityEngine;

using Genix.MocaLib.Runtime.Common;
using Genix.MocaLib.Runtime.Services;
using Genix.MocaLib.Runtime.Startup;
using UnityEngine.SceneManagement;

namespace Startup
{
    public class LoadingController : BaseLoadingController
    {
        protected override IEnumerator StartLoadingScreen()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(1);
            op.allowSceneActivation = false;

            bool isAppOpenAdShown = false;
            int step = 0;

            while (!_loadingDone)
            {
                _loadingTime += Time.deltaTime;

                switch (step)
                {
                    // Allow some "warm-up"
                    case 0:
                    case 1:
                    case 2:
                        step++;
                        break;

                    case 3:
#if UNITY_ANDROID
                        _versionText.text = $"Version: {GameVersionInfo.BUILD_VERSION}";
#elif UNITY_IOS
                        _versionText.text = $"Version: {GameVersionInfo.BUILD_VERSION} ({GameVersionInfo.BUILD_NUMBER})";
#endif

                        step++;
                        break;

                    case 4:
                        MocaLib.Instance.OnRemoteConfigFetchCompleted += () =>
                        {
                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_INTERSTITIAL_INTERVAL,
                                ref MocaLib.Instance.AdManager.AdConfig.InterstitialInterval);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteBool(
                                GameConstants.REMOTE_CONFIG_APP_OPEN_AD_ENABLED,
                                ref MocaLib.Instance.AdManager.AdConfig.IsAppOpenAdEnabled);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteBool(
                                GameConstants.REMOTE_CONFIG_CHEATS_ENABLED,
                                ref RemoteConfigs.Instance.GameConfigs.CheatsEnabled);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteBool(
                                GameConstants.REMOTE_CONFIG_INTERNET_CHECK,
                                ref RemoteConfigs.Instance.GameConfigs.InternetCheck);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteDouble(
                                GameConstants.REMOTE_CONFIG_AI_DIFFICULTY,
                                ref RemoteConfigs.Instance.GameConfigs.AIDifficulty);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_INITIAL_HINTS,
                                ref RemoteConfigs.Instance.GameConfigs.InitialHints);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_INITIAL_COINS,
                                ref RemoteConfigs.Instance.GameConfigs.InitialCoins);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_COINS_PER_AD,
                                ref RemoteConfigs.Instance.GameConfigs.CoinsPerAd);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_COINS_PER_GAME,
                                ref RemoteConfigs.Instance.GameConfigs.CoinsPerGame);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_RATING_SHOW_AT_MATCH,
                                ref RemoteConfigs.Instance.GameConfigs.RatingShowAtMatch);

                            MocaLib.Instance.RemoteConfigManager.GetRemoteInt(
                                GameConstants.REMOTE_CONFIG_ADS_START_FROM_MATCH,
                                ref RemoteConfigs.Instance.GameConfigs.AdsStartFromMatch);
                        };

                        MocaLib.Instance.Initialize();

                        step++;
                        break;

                    case 5:
                        if (RemoteConfigs.Instance.GameConfigs.CheatsEnabled)
                        {
                            SRDebug.Init();
                        }

                        GameManager.Instance.Initialize();

                        step++;
                        break;

                    case 6:
                        if (_loadingTime >= _maxLoadingTime)
                        {
                            _loadingTime = _maxLoadingTime;
                            _loadingDone = true;
                        }

                        break;
                }

                UpdateLoadingBarProgress(_loadingTime);

                yield return new WaitForEndOfFrame();
            }

            op.allowSceneActivation = true;
            op.completed += (operation) =>
            {
                UIManager.Instance.ToggleCoinBarAndHomeScreen(true);
                UIManager.Instance.ToggleCoinsAttractor(true);

                GameManager.Instance.InitializeAdSettings();
                NavigationBarController.Instance.Initialize();
                ShopManager.Instance.Initialize();
                RewardManager.Instance.Initialize();
                ThemeManager.Instance.Initialize();
            };
        }
    }
}
