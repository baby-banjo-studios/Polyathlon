using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class BananaItem : Item
    {
        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            Instantiate(Child, racer.ItemDropPoint, Quaternion.identity);
            racer.PlayMiscSound(soundWhenUsed);
            racer.EquipItem(null);
        }
    }
}