using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Race;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class PropelonItem : Item
    {
        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            // target can be null
            Racer target = RaceManager.GetClosestRacerAheadOfThisOne(racer);

            Vector3 pos = racer.GetItemSpawnPos();
            Quaternion startingRot = racer.characterMesh.rotation;
            if (target != null)
            {
                startingRot *= Quaternion.Euler(Vector3.left * 30f);
            }
            GameObject obj = Instantiate(Child, pos, startingRot);

            PropelonObject projectile = obj.GetComponent<PropelonObject>();

            if (target != null)
            {
                projectile.target = target.GetComponent<Rigidbody>();
            }
            projectile.source = racer.GetComponent<Rigidbody>();

            racer.EquipItem(null);
        }
    }
}