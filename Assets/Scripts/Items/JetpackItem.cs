using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class JetpackItem : Item
    {
        public override void Pickup(Racer racer)
        {
            racer.SetMovementMode(MovementMode.Jetpacking);
            base.Pickup(racer);
        }
    }
}