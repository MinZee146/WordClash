using UnityEngine;

[CreateAssetMenu(fileName = "Cleanse", menuName = "Powerups/Cleanse")]
public class Cleanse : PowerUpBase
{
    private void OnEnable()
    {
        Name = "Cleanse";
    }

    public override void ApplyPowerUp()
    {

    }
}
