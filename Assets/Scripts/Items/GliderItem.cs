using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class GliderItem : Item
    {
        public override void Pickup(Racer racer)
        {
            racer.SetMovementMode(MovementMode.Gliding);
            base.Pickup(racer);
        }
    }
}
