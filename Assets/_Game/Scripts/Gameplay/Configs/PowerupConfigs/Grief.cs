using UnityEngine;

[CreateAssetMenu(fileName = "Grief", menuName = "Powerups/Grief")]
public class Grief : PowerUpBase
{
    private void OnEnable()
    {
        Name = "Grief";
    }
    
    public override void ApplyPowerUp()
    {

    }
}

