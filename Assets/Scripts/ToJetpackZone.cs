using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.AI
{
// For selective use with a trigger.
// Enables forcing an NPC to go straight to the nearest jetpack
// Not currently in use, but may be useful in the future.
public class ToJetpackZone : MonoBehaviour
{
    private List<NPC> seenNPCs;

    void Start()
    {
        seenNPCs = new List<NPC>();
    }

    public void OnTriggerStay(Collider other)
    {
        NPC npc = other.GetComponent<NPC>();
        if (npc != null)  
        {
            if (!seenNPCs.Contains(npc))
            {
                seenNPCs.Add(npc);
                StartCoroutine(EnsureOnNavmesh(npc));
            }
        }
    }

    private IEnumerator EnsureOnNavmesh(NPC npc)
    {
        yield return new WaitForSeconds(2);
        if (npc.GetCurrentMovementMode() != MovementMode.Jetpacking)
        {
            npc.GoToNearestJetpack();
        }
    }
}
}