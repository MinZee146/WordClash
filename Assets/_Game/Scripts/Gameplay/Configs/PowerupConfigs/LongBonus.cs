using UnityEngine;

[CreateAssetMenu(fileName = "LongBonus", menuName = "Powerups/LongBonus")]
public class LongBonus : PowerUpBase
{
    private void OnEnable()
    {
        Name = "LongBonus";
    }
    
    public override void ApplyPowerUp()
    {
        AI.Instance.PreferLong = true;
    }
}
