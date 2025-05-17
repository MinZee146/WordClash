using UnityEngine;

[CreateAssetMenu(fileName = "DoubleScore", menuName = "Powerups/DoubleScore")]
public class DoubleScore : PowerUpBase
{
    private void OnEnable()
    {
        Name = "DoubleScore";
    }
    
    public override void ApplyPowerUp()
    {

    }
}
