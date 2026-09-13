using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class WheelerItem : Item
    {
        public override void Pickup(Racer racer)
        {
            racer.SetMovementMode(MovementMode.Wheeling);
            base.Pickup(racer);
        }
    }
}