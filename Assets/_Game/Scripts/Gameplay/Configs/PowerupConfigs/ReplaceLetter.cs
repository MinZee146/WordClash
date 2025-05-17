using UnityEngine;

[CreateAssetMenu(fileName = "LetterReplace", menuName = "Powerups/LetterReplace")]
public class LetterReplace : PowerUpBase
{
    private void OnEnable()
    {
        Name = "ReplaceLetter";
    }

    public override void ApplyPowerUp()
    {
        if (GameFlowManager.Instance.IsPlayerTurn)
        {
            Board.Instance.HandleTileReplace += () =>
            {
                PopUpsManager.Instance.ToggleLetterReplacePopUp(true);
            };
        }
    }
}