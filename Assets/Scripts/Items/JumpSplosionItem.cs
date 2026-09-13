using BabyBanjo.Polyathlon.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class JumpSplosionItem : Item
    {
        public float explosionRange;
        public float opponentForce;
        public float playerForce;
        public float playerLift;
        private Vector3 expStart;
        public GameObject explosionPrefab;

        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            expStart = racer.transform.position;
            Collider[] nearbyColliders = Physics.OverlapSphere(racer.transform.position, explosionRange/*, LayerMask.NameToLayer("Racer")*/);
            List<Racer> nearbyRacers = new List<Racer>();
            foreach (Collider coll in nearbyColliders)
            {
                if (coll.TryGetComponent<Racer>(out Racer otherRacer) && otherRacer != racer)
                {
                    nearbyRacers.Add(otherRacer);
                }
            }

            foreach (Racer otherRacer in nearbyRacers)
            {
                Vector3 force = (otherRacer.transform.position - racer.transform.position).normalized * opponentForce;
                otherRacer.Die(true, force);
            }

            Vector3 playerForceV3 = racer.Forward * playerForce;
            playerForceV3 = Quaternion.AngleAxis(-playerLift, racer.characterMesh.right) * playerForceV3;
            Debug.DrawRay(racer.transform.position, playerForceV3, Color.blue, 5f);
            //racer.Die(true, playerForceV3);
            racer.ApplyJumpSplosion(playerForceV3);

            Instantiate(explosionPrefab, racer.transform.position + Vector3.up * 0.8f, racer.transform.rotation);
            //explosionParticle.Play();

            racer.PlayMiscSound(soundWhenUsed);
            racer.EquipItem(null);
        }
    }
}