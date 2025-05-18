using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using MEC;
using System;
using UnityEngine.SceneManagement;

public class UIManager : SingletonPersistent<UIManager>
{
    [SerializeField] private GameObject _popupBG, _homeScreen, _loadingBG, _currency, _coinsAttractor;
    [SerializeField] private Sprite _soundOn, _soundOff;

    private AsyncOperationHandle<SceneInstance> _sceneStartupHandle;
    private AsyncOperationHandle<SceneInstance> _sceneGameplayHandle;
    private bool _isInspectingBoard, _isInteractable = true;
    private int _activePanelCount = 0;

    public CanvasGroup UICanvasGroup;
    public string InspectPanel { get; private set; }
    public bool IsInspectingBoard
    {
        get => _isInspectingBoard;
        set => _isInspectingBoard = value;
    }
    public bool IsInteractable
    {
        get => _isInteractable;
        set => _isInteractable = value;
    }

    public void Initialize()
    {
        NameRegister.Instance.Initialize();
        LoadingAnimation.Instance.Initialize();
    }

    public bool CheckCanInteractBoard()
    {
        if (_popupBG.activeSelf || _loadingBG.activeSelf || !_isInteractable)
        {
            return false;
        }

        return true;
    }

    #region LoadScene
    public void LoadGameScene()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
        {
            _sceneGameplayHandle = Addressables.LoadSceneAsync("Assets/_Game/Scenes/Gameplay.unity");
            _sceneGameplayHandle.Completed += handle =>
            {
                if (_sceneStartupHandle.IsValid())
                {
                    Addressables.UnloadSceneAsync(_sceneStartupHandle, true);
                }

                ToggleCoinBar(false);

                GameManager.Instance.NewGame();
                HintCounter.Instance.FetchHintPref();
                PopUpsPool.Instance.Instantiate();

                LoadingAnimation.Instance.AnimationLoaded(0.5f, 0.25f);
                _homeScreen.SetActive(false);
            };
        });
    }

    public void LoadTimeModeScene()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
        {
            _sceneGameplayHandle = Addressables.LoadSceneAsync("Assets/_Game/Scenes/TimeChallengeMode.unity");
            _sceneGameplayHandle.Completed += handle =>
            {
                if (_sceneStartupHandle.IsValid())
                {
                    Addressables.UnloadSceneAsync(_sceneStartupHandle, true);
                }

                ToggleCoinBar(false);

                GameUIController.Instance.ToggleHintAndConfirm();
                HintCounter.Instance.FetchHintPref();
                PopUpsPool.Instance.Instantiate();
                PlayerStatsManager.Instance.ResetStats();

                LoadingAnimation.Instance.AnimationLoaded(0.5f, 0.25f);
                _homeScreen.SetActive(false);
            };
        });
    }

    public void LoadMenuScene()
    {
        DisableAllPanel();
        AudioManager.Instance.StopSideAudio();

        LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
        {
            _sceneStartupHandle = Addressables.LoadSceneAsync("Assets/_Game/Scenes/Home.unity");
            _sceneStartupHandle.Completed += handle =>
            {
                if (_sceneGameplayHandle.IsValid())
                {
                    Addressables.UnloadSceneAsync(_sceneGameplayHandle, true);
                }

                ToggleCoinBar(true);

                LoadingAnimation.Instance.AnimationLoaded(0.5f, 0.25f);
                _homeScreen.SetActive(true);
            };
        });
    }

    public void ToggleCoinBar(bool state)
    {
        ToggleCurrency(state);
    }
    #endregion

    #region TogglePanel
    public void PanelAnimation(GameObject panel, bool setActive, Action onOpen = null, Action onClose = null)
    {
        panel.transform.DOKill();
        if (setActive)
        {
            onOpen?.Invoke();
            panel.SetActive(true);
            SetBGState(true);

            panel.transform.localScale = Vector3.zero;
            panel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                UICanvasGroup.blocksRaycasts = true;
            });
        }
        else
        {
            panel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                onClose?.Invoke();
                panel.SetActive(false);
                SetBGState(false);

                if (SceneManager.GetActiveScene().name == "Gameplay")
                {
                    GameUIController.Instance.GameplayCanvasGroup.blocksRaycasts = true;
                }

                UICanvasGroup.blocksRaycasts = true;
            });
        }
    }

    public void DisableAllPanel()
    {
        _isInspectingBoard = false;

        if (_popupBG.activeSelf)
        {
            PopUpsManager.Instance.CloseAllPopUps();
        }
    }

    #endregion

    #region ToggleUI
    public void SetButtonInGameOverActive(GameObject homeButton, GameObject replayButton)
    {
        homeButton.SetActive(true);
        replayButton.SetActive(true);

        var homeImage = homeButton.GetComponent<Image>();
        var replayImage = replayButton.GetComponent<Image>();

        homeImage.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
        replayImage.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
    }

    public void ToggleCurrency(bool setActive)
    {
        _currency.SetActive(setActive);
    }

    public void ToggleInspectPowerUps()
    {
        _isInspectingBoard = !_isInspectingBoard;
        InspectPanel = "PowerUps";

        PopUpsManager.Instance.TogglePowerupsPopUp(!_isInspectingBoard);
    }

    public void ToggleInspectReplace()
    {
        _isInspectingBoard = !_isInspectingBoard;
        InspectPanel = "Replace";

        PopUpsManager.Instance.ToggleLetterReplacePopUp(!IsInspectingBoard);
    }

    public void ToggleCoinsAttractor(bool setActive)
    {
        _coinsAttractor.SetActive(setActive);
    }

    private void SetBGState(bool setActive)
    {
        if (setActive)
        {
            if (_activePanelCount == 0)
            {
                _popupBG.SetActive(true);
            }

            _activePanelCount++;
        }
        else
        {
            _activePanelCount--;

            if (_activePanelCount == 0)
            {
                _popupBG.SetActive(false);
            }
        }
    }

    #endregion

    #region SettingsUI
    public void ToggleSFX(GameObject sfxButton)
    {
        AudioManager.Instance.ToggleSFX();
        var isSfxOn = sfxButton.GetComponent<Image>().sprite == _soundOn;
        sfxButton.GetComponent<Image>().sprite = isSfxOn ? _soundOff : _soundOn;

        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_IS_SFX_ON, isSfxOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    public void ToggleMusic(GameObject musicButton)
    {
        AudioManager.Instance.ToggleMusic();
        var isMusicOn = musicButton.GetComponent<Image>().sprite == _soundOn;
        musicButton.GetComponent<Image>().sprite = isMusicOn ? _soundOff : _soundOn;

        PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_IS_MUSIC_ON, isMusicOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    public void UpdateSettingsUI(GameObject sfxButton, GameObject musicButton)
    {
        var sfxState = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_IS_SFX_ON, 1);
        sfxButton.GetComponent<Image>().sprite = sfxState == 1 ? _soundOn : _soundOff;

        var musicState = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_IS_MUSIC_ON, 1);
        musicButton.GetComponent<Image>().sprite = musicState == 1 ? _soundOn : _soundOff;
    }
    #endregion

    #region TextPopUps
    public void InstantiatePopUps(string word)
    {
        switch (word.Length)
        {
            case 3:
                PopUpsPool.Instance.SpawnFromPool("Great");
                break;
            case 4:
                PopUpsPool.Instance.SpawnFromPool("Amazing");
                break;
            case 5:
            case 6:
                PopUpsPool.Instance.SpawnFromPool("Fabulous");
                break;
            case 7:
            case 8:
                PopUpsPool.Instance.SpawnFromPool("Spectacular");
                break;
            default:
                Utils.LogWarning("No pop-up available for the given word length.");
                break;
        }
    }

    public IEnumerator<float> ShowPopUp(string selectedWord, int score)
    {
        if (GameFlowManager.Instance.IsPlayerTurn && selectedWord.Length > 2)
        {
            if (selectedWord == WordFinder.Instance.BestWord)
            {
                BestWordPopUp($"{selectedWord} ({score})");
            }

            AudioManager.Instance.PlaySFX("Compliment");
            InstantiatePopUps(selectedWord);

            yield return Timing.WaitForSeconds(2f);
        }
    }

    public void BestWordPopUp(string word)
    {
        Addressables.LoadAssetAsync<GameObject>("bestword").Completed += handle =>
        {
            var popUp = Instantiate(handle.Result);
            popUp.transform.SetParent(transform, false);
            popUp.GetComponent<BestWordAnimation>().SetProps(word, handle);
            popUp.SetActive(true);

            var sequence = DOTween.Sequence();
            sequence.AppendInterval(1.8f);
            sequence.Append(popUp.transform.DOScale(Vector3.zero, 0.5f));
            sequence.Play().OnComplete(() => popUp.GetComponent<BestWordAnimation>().CleanUp());
        };
    }
    #endregion
}
