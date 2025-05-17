using UnityEngine;

[CreateAssetMenu(fileName = "ShortBonus", menuName = "Powerups/ShortBonus")]
public class ShortBonus : PowerUpBase
{
    private void OnEnable()
    {
        Name = "ShortBonus";
    }
    
    public override void ApplyPowerUp()
    {
        AI.Instance.PreferShort = true;
    }
}
