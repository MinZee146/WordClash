using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RevealWord", menuName = "Powerups/RevealWord")]
public class RevealWord : PowerUpBase
{
    private void OnEnable()
    {
        Name = "RevealWord";
    }

    public override void ApplyPowerUp()
    {
        var word = Board.Instance.FoundWords.Keys.OrderByDescending(word => word.Length).FirstOrDefault();

        if (!GameFlowManager.Instance.IsPlayerTurn)
        {
            AI.Instance.ForcedWord = word;
        }
        else
        {
            PopUpsManager.Instance.ToggleRevealWordPopUp(true);
        }
    }
}
