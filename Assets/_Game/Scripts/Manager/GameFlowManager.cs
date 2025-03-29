using System.Collections.Generic;
using Genix.MocaLib.Runtime.Services;
using MEC;
using UnityEngine;

public class GameFlowManager : SingletonPersistent<GameFlowManager>
{
    public bool IsPlayerTurn { get; private set; }
    public int Turn { get; private set; }
    public int Round { get; private set; }

    public void StartGame()
    {
        MocaLib.Instance.AnalyticsManager.LogEvent("round_start", new Dictionary<string, object>
        {
            { "play_mode", GameManager.Instance.CurrentGameMode }
        });

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
            MocaLib.Instance.AnalyticsManager.LogEvent("round_end", new Dictionary<string, object>
            {
                { "play_mode", GameManager.Instance.CurrentGameMode },
                { "success", PlayerStatsManager.Instance.IsPlayerWon() },
                { "reason", PlayerStatsManager.Instance.HasWonRound() ? "None" : "Less Score" },
                { "player_score", PlayerStatsManager.Instance.PlayerScoreAtRound },
                { "opponent_score", PlayerStatsManager.Instance.OpponentScoreAtRound },
                { "time_percent_used_per_turn", Notifier.Instance.AverageTimePercentUsedAtRound },
                { "number_of_time_up", Notifier.Instance.TimesUpAtRound },
                { "hints_used", HintCounter.Instance.HintUsedAtRound },
            });

            NextRound();
        }
        else
        {
            MocaLib.Instance.AnalyticsManager.LogEvent("round_end", new Dictionary<string, object>
            {
                { "play_mode", GameManager.Instance.CurrentGameMode },
                { "success", PlayerStatsManager.Instance.IsPlayerWon() },
                { "reason", PlayerStatsManager.Instance.HasWonRound() ? "None" : "Less Score" },
                { "player_score", PlayerStatsManager.Instance.PlayerScoreAtRound },
                { "opponent_score", PlayerStatsManager.Instance.OpponentScoreAtRound },
                { "time_percent_used_per_turn", Notifier.Instance.AverageTimePercentUsedAtRound },
                { "number_of_time_up", Notifier.Instance.TimesUpAtRound },
                { "hints_used", HintCounter.Instance.HintUsedAtRound },
            });

            MocaLib.Instance.AnalyticsManager.LogEvent("level_end", new Dictionary<string, object>
            {
                { "play_mode", GameManager.Instance.CurrentGameMode },
                { "success", PlayerStatsManager.Instance.IsPlayerWon() },
                { "reason", PlayerStatsManager.Instance.IsPlayerWon() ? "None" : "Less Score" },
                { "ad_duration", RewardManager.Instance.TotalAdDuration },
                { "player_score", PlayerStatsManager.Instance.PlayerScore },
                { "opponent_score", PlayerStatsManager.Instance.OpponentScore },
                { "time_percent_used_per_turn", Notifier.Instance.AverageTimePercentUsed },
                { "number_of_time_up", Notifier.Instance.TotalTimesUp },
                { "hints_used", HintCounter.Instance.TotalHintUsed },
            });

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
