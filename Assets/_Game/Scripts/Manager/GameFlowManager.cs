using System.Collections.Generic;
using MEC;
using UnityEngine;

public class GameFlowManager : SingletonPersistent<GameFlowManager>
{
    public bool IsPlayerTurn { get; private set; }
    public int Turn { get; private set; }
    public int Round { get; private set; }

    public void StartGame()
    {
        Round = 1;
        Turn = 1;
        IsPlayerTurn = true;

        GameUIController.Instance.UpdateRoundIndicator();
        GameUIController.Instance.ToggleHintAndConfirm();
    }

    public void NextRound()
    {
        Round++;
        Turn = 0;
        IsPlayerTurn = Round != 1;

        PopUpsManager.Instance.ToggleRoundChangePanel(true);
    }

    public void HandleGameOver()
    {
        Timing.KillCoroutines("AI");
        HintCounter.Instance.SetStatsAtRound();
        Notifier.Instance.SetStatsAtRound();

        if (Round == 1)
        {
            NextRound();
        }
        else
        {
            PopUpsManager.Instance.ToggleGameOverPopUp(true);
            PlayerStatsManager.Instance.LogStats();
            AudioManager.Instance.PlaySFX("Bell");

            var totalMatchPlayed = PlayerPrefs.GetInt(GameConstants.PLAYER_PREFS_TOTAL_MATCH_PLAYED, 0);
            PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_TOTAL_MATCH_PLAYED, totalMatchPlayed + 1);
            PlayerPrefs.Save();
        }
    }

    public void NextTurn()
    {
        Turn++;
        IsPlayerTurn = !IsPlayerTurn;

        GameUIController.Instance.ToggleHintAndConfirm();
        Notifier.Instance.OnTurnChanged();
        PopUpsManager.Instance.CloseCurrentPopUp();
        BottomBar.Instance.SetSidePowerUpState(IsPlayerTurn);

        if (IsPlayerTurn)
        {
            if (Turn > 2 && PowerUpsManager.Instance.PowerUpCounts() > 0)
            {
                PopUpsManager.Instance.TogglePowerupsPopUp(true);
            }
        }
        else
        {
            Timing.RunCoroutine(AI.Instance.AITurn(), "AI");
        }
    }
}
