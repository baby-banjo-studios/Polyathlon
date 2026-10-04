using UnityEngine;

public class BikeItem : Item
{
    public override void Pickup(Racer racer)
    {
        racer.SetMovementMode(Movement.Mode.Biking);
        base.Pickup(racer);

        if (dontRespawn)
        {
            Destroy(this.gameObject);
        }
    }
}
