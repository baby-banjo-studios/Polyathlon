using System.Collections.Generic;
using UnityEngine;

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
        List<Entity> nearbyTargets = new List<Entity>();
        foreach (Collider coll in nearbyColliders)
        {
            if (coll.TryGetComponent<Racer>(out Racer otherRacer) && otherRacer != racer)
            {
                nearbyTargets.Add(otherRacer);
            }    
        }

        foreach (Entity other in nearbyTargets)
        {
            Vector3 force = (other.transform.position - racer.transform.position).normalized * opponentForce;
            other.Die(true, force);
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