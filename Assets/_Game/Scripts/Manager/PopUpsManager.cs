using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PopUpsManager : Singleton<PopUpsManager>
{
    [SerializeField] private AssetReference _revealWord, _opponentPowerUp, _roundChange, _purchaseFailed, _purchaseCompleted, _theme, _dailyReward, _stats, _credits, _replaceLetter, _doubleReward, _moreHints, _instruction, _nameRegister, _avatar, _settings, _powerups, _gameOver;

    private GameObject _currentPopUp, _subPopUp;
    private AsyncOperationHandle<GameObject> _currentHandle, _subHandle;
    private AssetReference _currentRef, _subRef;
    private Button _replayButton, _homeButton;

    public Button ReplayButton => _replayButton;
    public Button HomeButton => _homeButton;

    public void ToggleGameOverPopUp(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        UIManager.Instance.ToggleCurrency(setActive);

        TogglePopUp(_gameOver, setActive,
        onLoaded: (popUp) =>
        {
            _replayButton = popUp.transform.GetChild(1).GetComponent<Button>();
            _replayButton.onClick.AddListener(() =>
            {
                GameManager.Instance.Replay();
            });

            _homeButton = popUp.transform.GetChild(2).GetComponent<Button>();
            _homeButton.onClick.AddListener(() =>
            {
                UIManager.Instance.LoadMenuScene();
            });

            var playerStats = popUp.transform.GetChild(3);
            playerStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerStatsManager.Instance.PlayerName;
            playerStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Best word: {PlayerStatsManager.Instance.GetPlayerBestWord()}";

            var opponentStats = popUp.transform.GetChild(4);
            opponentStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerStatsManager.Instance.OpponentName;
            opponentStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Best word: {PlayerStatsManager.Instance.GetOpponentBestWord()}";
        },
        onClosed: () =>
        {
            _replayButton = _homeButton = null;
        });
    }

    public void TogglePowerupsPopUp(bool setActive)
    {
        TogglePopUp(_powerups, setActive,
        onLoaded: (popUp) =>
        {
            Notifier.Instance.PauseCountdown();

            var powerupList = new List<Button>();
            for (var i = 0; i < 6; i++)
            {
                var button = popUp.transform.GetChild(i).GetComponent<Button>();

                button.onClick.AddListener(() =>
                {
                    PowerUpsManager.Instance.SetPowerUpButtonState(button, false);
                });

                powerupList.Add(button);
            }

            PowerUpsManager.Instance.UpdateButtons(powerupList);

            var inspectButton = popUp.transform.GetChild(7).GetComponent<Button>();
            inspectButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ToggleInspectPowerUps();
            });
        },
        onClosed: () =>
        {
            Notifier.Instance.ResumeCountdown();
        });
    }

    public void ToggleSettingsPopUp(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        TogglePopUp(_settings, setActive,
        onLoaded: (popUp) =>
        {
            popUp.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleSettingsPopUp(false);
            });

            var creditButton = popUp.transform.GetChild(5);
            creditButton.gameObject.SetActive(SceneManager.GetActiveScene().name == "Home");
            creditButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleCreditsPopUp(true);
            });

            var homeButton = popUp.transform.GetChild(2);
            homeButton.gameObject.SetActive(SceneManager.GetActiveScene().name == "Gameplay");
            homeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Timing.KillCoroutines();
                DOTween.KillAll();
                UIManager.Instance.IsInteractable = true;
                UIManager.Instance.LoadMenuScene();
                ToggleSettingsPopUp(false);
            });

            var musicButton = popUp.transform.GetChild(0).gameObject;
            musicButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                UIManager.Instance.ToggleMusic(musicButton);
            });

            var sfxButton = popUp.transform.GetChild(1).gameObject;
            sfxButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                UIManager.Instance.ToggleSFX(sfxButton);
            });

            UIManager.Instance.UpdateSettingsUI(sfxButton, musicButton);
        });
    }

    public void ToggleAvatarPopUp(bool setActive)
    {
        TogglePopUp(_avatar, setActive,
        onLoaded: (popUp) =>
        {
            var buttonList = popUp.GetComponentsInChildren<Button>();
            foreach (var button in buttonList)
            {
                button.onClick.AddListener(() =>
                {
                    ToggleAvatarPopUp(false);
                    AudioManager.Instance.PlaySFX("ButtonClick");
                });
            }
        },
        onClosed: () =>
        {
            UIManager.Instance.ToggleCurrency(true);
        });
    }

    public void ToggleNameRegisterPopUp(bool setActive, bool changeAvatar = false)
    {
        if (PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_USERNAME))
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        TogglePopUp(_nameRegister, setActive,
        onLoaded: (popUp) =>
        {
            var closeButton = popUp.transform.GetChild(3).GetComponent<Button>();
            closeButton.gameObject.SetActive(PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_USERNAME));

            closeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleNameRegisterPopUp(false);
            });

            var usernameInput = popUp.GetComponentInChildren<TMP_InputField>();

            popUp.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                NameRegister.Instance.ConfirmUsername(usernameInput);
            });
        },
        onClosed: () =>
        {
            if (changeAvatar)
            {
                ToggleAvatarPopUp(true);
            }
        });
    }

    public void ToggleInstructionPopUp(bool setActive)
    {
        TogglePopUp(_instruction, setActive);
    }

    public void ToggleMoreHintsPopUp(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        UIManager.Instance.ToggleCurrency(setActive);
        TogglePopUp(_moreHints, setActive,
        onLoaded: (popUp) =>
        {
            popUp.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleMoreHintsPopUp(false);
            });

            popUp.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                RewardManager.Instance.GrantHints(popUp.transform.GetChild(4).gameObject);
            });

            Notifier.Instance.PauseCountdown();
        },
        onClosed: () =>
        {
            Notifier.Instance.ResumeCountdown();
            UIManager.Instance.ToggleCurrency(false);
        });
    }

    public void ToggleLetterReplacePopUp(bool setActive, Action<TMP_InputField> onAIReplaceLetter = null)
    {
        TogglePopUp(_replaceLetter, setActive,
        onLoaded: (popUp) =>
        {
            var replaceLetter = GetComponentInChildren<TMP_InputField>();
            var confirmButton = popUp.transform.GetChild(2).GetComponent<Button>();
            var inspectButton = popUp.transform.GetChild(1).GetComponent<Button>();

            onAIReplaceLetter?.Invoke(replaceLetter);

            confirmButton.onClick.AddListener(() =>
            {
                ToggleLetterReplacePopUp(false);
                Board.Instance.ReplaceSelectingTileWith(replaceLetter.text[0]);
                Board.Instance.ClearHandleTileReplaceListeners();
                Notifier.Instance.OnTurnChanged();
            });

            inspectButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ToggleInspectReplace();
            });

            replaceLetter.onValueChanged.AddListener((value) =>
            {
                if (GameFlowManager.Instance.IsPlayerTurn)
                {
                    inspectButton.transform.DOKill();
                    confirmButton.transform.DOKill();

                    if (string.IsNullOrWhiteSpace(replaceLetter.text))
                    {
                        inspectButton.gameObject.SetActive(true);
                        inspectButton.transform.DOScale(Vector3.one, 0.15f);
                        confirmButton.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => confirmButton.gameObject.SetActive(false));
                    }
                    else
                    {
                        confirmButton.gameObject.SetActive(true);
                        confirmButton.transform.DOScale(Vector3.one, 0.15f);
                        inspectButton.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => inspectButton.gameObject.SetActive(false));
                    }
                }
                else
                {
                    confirmButton.gameObject.SetActive(false);
                    inspectButton.gameObject.SetActive(false);
                }
            });

            Notifier.Instance.PauseCountdown();
            replaceLetter.text = "";

            if (!GameFlowManager.Instance.IsPlayerTurn)
            {
                replaceLetter.readOnly = true;
                confirmButton.gameObject.SetActive(false);
                inspectButton.gameObject.SetActive(false);
            }
            else
            {
                replaceLetter.readOnly = false;
                inspectButton.gameObject.SetActive(true);
            }
        },
        onClosed: () =>
        {
            Notifier.Instance.ResumeCountdown();
            AudioManager.Instance.PlaySFX("ButtonClick");
        });
    }

    public void ToggleCreditsPopUp(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        TogglePopUp(_credits, setActive,
        onLoaded: (popUp) =>
        {
            UIManager.Instance.ToggleCoinBarAndHomeScreen(false);

            popUp.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                ToggleCreditsPopUp(false);
                UIManager.Instance.ToggleCoinBarAndHomeScreen(true);
            });
        });
    }

    public void ToggleStatsPopUp(bool setActive, Action changeNameAndAvatar = null)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        TogglePopUp(_stats, setActive,
        onLoaded: (popUp) =>
        {
            popUp.transform.GetChild(10).GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleStatsPopUp(false);
            });

            popUp.transform.GetChild(0).GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                NameRegister.Instance.ChangeNameAndAvatar();
            });

            LoadStats.Instance.Load();
        },
        onClosed: () =>
        {
            changeNameAndAvatar?.Invoke();
        });
    }

    public void ToggleDailyRewardPopUp(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        TogglePopUp(_dailyReward, setActive,
        onLoaded: (popUp) =>
        {
            popUp.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleDailyRewardPopUp(false);
            });
        });
    }

    public void ToggleThemePopUp(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        TogglePopUp(_theme, setActive,
        onLoaded: (popUp) =>
        {
            popUp.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                ToggleThemePopUp(false);
            });
        });
    }

    public void ToggleRevealWordPopUp(bool setActive)
    {
        TogglePopUp(_revealWord, setActive,
        onLoaded: (popUp) =>
        {
            var word = Board.Instance.FoundWords.Keys.OrderByDescending(word => word.Length).FirstOrDefault();
            popUp.GetComponentInChildren<TextMeshProUGUI>().text = $"The longest word available is\n\n<color=#ff8811>{word.ToUpper()}</color>";

            var confirmButton = popUp.GetComponentInChildren<Button>();
            confirmButton.onClick.AddListener(() => ToggleRevealWordPopUp(false));
            confirmButton.gameObject.SetActive(GameFlowManager.Instance.IsPlayerTurn);
        },
        onClosed: () =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        });
    }

    public void ToggleOpponentPowerupPopUp(bool setActive, string description, Sprite image)
    {
        TogglePopUp(_opponentPowerUp, setActive,
        onLoaded: (popUp) =>
        {
            var powerup = popUp.transform.GetChild(1);
            powerup.GetComponentInChildren<TextMeshProUGUI>().text = description;
            powerup.GetComponentsInChildren<Image>().First(c => c.gameObject != powerup.gameObject).sprite = image;
        });
    }

    public void ToggleRoundChangePanel(bool setActive)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        TogglePopUp(_roundChange, setActive,
        onLoaded: (popUp) =>
        {
            popUp.GetComponentInChildren<Button>().onClick.AddListener(() => ToggleRoundChangePanel(false));
        },
        onClosed: () =>
        {
            LoadingAnimation.Instance.AnimationLoading(0.5f, () =>
            {
                Board.Instance.NewGame();
                GameUIController.Instance.UpdateRoundIndicator();
                GameFlowManager.Instance.NextTurn();
                LoadingAnimation.Instance.AnimationLoaded(0.5f, 0);
            });
        });
    }

    public void TogglePurchaseCompletedPopUp(bool setActive, Action onClose = null)
    {
        TogglePopUp(_purchaseCompleted, setActive,
        onLoaded: (popUp) =>
        {
            popUp.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                TogglePurchaseCompletedPopUp(false);
                onClose?.Invoke();
            });
        });
    }

    public void TogglePurchaseFailedPopUp(bool setActive)
    {
        TogglePopUp(_purchaseFailed, setActive,
        onLoaded: (popUp) =>
        {
            popUp.GetComponentInChildren<Button>().onClick.AddListener(() => TogglePurchaseFailedPopUp(false));
        });
    }

    public void TogglePopUp(AssetReference ar, bool setActive, Action<GameObject> onLoaded = null, Action onClosed = null)
    {
        if (SceneManager.GetActiveScene().name == "Gameplay")
        {
            Board.Instance.IsDragging = false;
            GameUIController.Instance.GameplayCanvasGroup.blocksRaycasts = false;
        }
        else if (SceneManager.GetActiveScene().name == "Home")
        {
            HomeUIController.Instance.HomeCanvasGroup.blocksRaycasts = false;
        }

        UIManager.Instance.UICanvasGroup.blocksRaycasts = false;

        if (setActive)
        {
            if (_currentRef == ar || _subRef == ar)
            {
                return;
            }

            if (!_currentHandle.IsValid() || _currentPopUp == null)
            {
                _currentRef = ar;
                LoadPopUp(ar, popUp =>
                {
                    _currentPopUp = popUp;
                    UIManager.Instance.PanelAnimation(popUp, true);
                    onLoaded?.Invoke(popUp);
                });
            }
            else if (!_subHandle.IsValid() || _subPopUp == null)
            {
                _subRef = ar;
                LoadPopUp(ar, popUp =>
                {
                    _subPopUp = popUp;
                    UIManager.Instance.PanelAnimation(popUp, true);
                    onLoaded?.Invoke(popUp);
                });
            }
        }
        else
        {
            if (_subHandle.IsValid() && _subPopUp != null)
            {
                UIManager.Instance.PanelAnimation(_subPopUp, false, onClose: () =>
                {
                    Destroy(_subPopUp);
                    Addressables.Release(_subHandle);
                    _subPopUp = null;
                    _subHandle = default;
                    _subRef = null;
                    onClosed?.Invoke();
                });
            }
            else if (_currentHandle.IsValid() && _currentPopUp != null)
            {
                UIManager.Instance.PanelAnimation(_currentPopUp, false, onClose: () =>
                {
                    Destroy(_currentPopUp);
                    Addressables.Release(_currentHandle);
                    _currentPopUp = null;
                    _currentHandle = default;
                    _currentRef = null;
                    onClosed?.Invoke();
                });
            }
        }
    }

    private void LoadPopUp(AssetReference ar, Action<GameObject> onLoaded)
    {
        Addressables.LoadAssetAsync<GameObject>(ar).Completed += handle =>
        {
            if (!_currentHandle.IsValid() || _currentPopUp == null)
            {
                _currentHandle = handle;
                var popUp = Instantiate(handle.Result, transform, false);
                onLoaded?.Invoke(popUp);
            }
            else
            {
                _subHandle = handle;
                var popUp = Instantiate(handle.Result, transform, false);
                onLoaded?.Invoke(popUp);

                if (_currentRef == _settings && _subRef != _credits)
                {
                    popUp.transform.SetSiblingIndex(_currentPopUp.transform.GetSiblingIndex() - 1);
                }
            }
        };
    }

    public void CloseCurrentPopUp()
    {
        if (_subHandle.IsValid() && _subPopUp != null)
        {
            UIManager.Instance.PanelAnimation(_subPopUp, false, onClose: () =>
            {
                Destroy(_subPopUp);
                Addressables.Release(_subHandle);
                _subPopUp = null;
                _subHandle = default;
                _subRef = null;
            });
        }
        else if (_currentHandle.IsValid() && _currentPopUp != null)
        {
            UIManager.Instance.PanelAnimation(_currentPopUp, false, onClose: () =>
            {
                Destroy(_currentPopUp);
                Addressables.Release(_currentHandle);
                _currentPopUp = null;
                _currentHandle = default;
                _currentRef = null;
            });
        }
    }

    public void CloseAllPopUps()
    {
        if (_subHandle.IsValid() && _subPopUp != null)
        {
            UIManager.Instance.PanelAnimation(_subPopUp, false, onClose: () =>
            {
                Destroy(_subPopUp);
                Addressables.Release(_subHandle);
                _subPopUp = null;
                _subHandle = default;
                _subRef = null;
            });
        }

        if (_currentHandle.IsValid() && _currentPopUp != null)
        {
            UIManager.Instance.PanelAnimation(_currentPopUp, false, onClose: () =>
            {
                Destroy(_currentPopUp);
                Addressables.Release(_currentHandle);
                _currentPopUp = null;
                _currentHandle = default;
                _currentRef = null;
            });
        }
    }
}
