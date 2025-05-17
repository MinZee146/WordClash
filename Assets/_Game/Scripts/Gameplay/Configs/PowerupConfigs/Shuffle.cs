using UnityEngine;

[CreateAssetMenu(fileName = "Shuffle", menuName = "Powerups/Shuffle")]
public class Shuffle : PowerUpBase
{
    private void OnEnable()
    {
        Name = "Shuffle";
    }

    public override void ApplyPowerUp()
    {
        Board.Instance.ShuffleBoard();
    }
}