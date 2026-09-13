using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class SpiedieItem : Item
    {
        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            racer.SpeedBoost();
            racer.PlayMiscSound(soundWhenUsed);
            racer.EquipItem(null);
        }
    }
}