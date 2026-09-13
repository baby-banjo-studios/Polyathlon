using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class BikeItem : Item
    {
        public override void Pickup(Racer racer)
        {
            racer.SetMovementMode(MovementMode.Biking);
            base.Pickup(racer);
        }
    }
}