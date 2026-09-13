using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class StopSignItem : Item
    {
        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            Vector3 groundDropPoint = racer.ItemDropPoint;
            if (Physics.Raycast(racer.ItemDropPoint, Vector3.down, out RaycastHit hit))
            {
                groundDropPoint = hit.point;
            }

            Quaternion rot = Quaternion.LookRotation(racer.Forward);
            StopSignObject obj = Instantiate(Child, groundDropPoint, rot).GetComponent<StopSignObject>();
            racer.EquipItem(null);

            obj.Initialize(racer, racer.movementMode == MovementMode.Jetpacking || racer.movementMode == MovementMode.Gliding);

            racer.PlayMiscSound(soundWhenUsed);
            racer.EquipItem(null);
        }
    }
}