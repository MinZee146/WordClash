using UnityEngine;

[CreateAssetMenu(fileName = "ShortPenalty", menuName = "Powerups/ShortPenalty")]
public class ShortPenalty : PowerUpBase
{
    private void OnEnable()
    {
        Name = "ShortPenalty";
    }
    
    public override void ApplyPowerUp()
    {

    }
}