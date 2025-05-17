using UnityEngine;

[CreateAssetMenu(fileName = "ExtraTurn", menuName = "Powerups/ExtraTurn")]
public class ExtraTurn : PowerUpBase
{
    private void OnEnable()
    {
        Name = "ExtraTurn";
    }
    
    public override void ApplyPowerUp()
    {

    }
}

