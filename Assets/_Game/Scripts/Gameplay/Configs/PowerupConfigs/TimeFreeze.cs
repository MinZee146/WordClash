using UnityEngine;

[CreateAssetMenu(fileName = "TimeFreeze", menuName = "Powerups/TimeFreeze")]
public class TimeFreeze : PowerUpBase
{
    private void OnEnable()
    {
        Name = "TimeFreeze";
    }

    public override void ApplyPowerUp()
    {
        Notifier.Instance.PauseCountdown();
    }
}
