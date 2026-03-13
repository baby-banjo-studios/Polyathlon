using UnityEngine;

public class WheelerItem : Item
{
    public override void Pickup(Racer racer)
    {
        racer.SetMovementMode(Movement.Mode.Wheeling);
        base.Pickup(racer);
    }
}
