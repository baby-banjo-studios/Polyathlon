using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class MelonItem : Item
    {
        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            Vector3 pos = racer.GetItemSpawnPos();
            GameObject obj = Instantiate(Child, pos, Quaternion.identity);
            Rigidbody itemRb = obj.GetComponent<Rigidbody>();

            itemRb.linearVelocity = racer.Speed * racer.Forward;
            itemRb.AddForce(1000 * (racer.characterMesh.transform.forward + 0.1f * transform.up));

            racer.EquipItem(null);
        }
    }
}