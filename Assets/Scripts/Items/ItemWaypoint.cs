using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ItemWaypoint : MonoBehaviour, IWaypointable
{
    public float chanceNPCGoesForItem = 1; // 0 means they will not go for an item, 1 means they definitely will
    public float chanceNPCWaitsForItem = 1; // same scale as above, should the NPC wait at the item's spot until it is available?
    public Waypoint[] waypointFork; // Unity doesn't let you serialize interfaces :(
    public ItemWaypoint[] itemWaypointFork;
    public JumpOffWaypoint[] jumpOffWaypointFork;
    public IWaypointable Next { get { return next; } set { next = value; } }
    private IWaypointable next;
    public int Seq { get { return seq; } set { seq = value; } }
    private int seq;
    private Item[] items;
    private Dictionary<NPC, Item> currentClaimers;

    // Start is called before the first frame update
    private void Awake()
    {
        items = GetComponentsInChildren<Item>();
        foreach(Item item in items)
        {
            item.AssignItemWaypoint(this);
        }
        currentClaimers = new Dictionary<NPC, Item>();
    }

    // This will decide and inform the NPC of which item it should go for
    public Vector3 GetPos(NPC npc)
    {
        if (!currentClaimers.ContainsKey(npc))
        {
            // Determine if we should go for an item or not
            if (Random.Range(0f, 1f) > chanceNPCGoesForItem)
            {
                npc.ArriveAtWaypoint(this);
                // If the item waypoint is the last one in a chain and has a guarenteed fork,
                // then we must consult the fork
                if (Next == null)
                {
                    return GetFork()[Random.Range(0, GetFork().Length)].GetPos(npc);
                }
                return Next.GetPos(npc);
            }
            else
            {
                int i = 0;
                // Find an item that hasn't been claimed
                while (i < items.Length && currentClaimers.ContainsValue(items[i]))
                {
                    i++;
                }
                // if all items are claimed already, pick one at random
                if (i >= items.Length)
                {
                    i = Random.Range(0, items.Length);
                }
                // Claim the item for the NPC
                currentClaimers.Add(npc, items[i]);
            }
        }
        // return the position of the NPC's assigned item
        return currentClaimers[npc].transform.position;
    }

    // Handle the arrival of the NPC at the waypoint and take them off the dictionary
    public void NPCTookItem(NPC npc, Item item)
    {
        npc.ArriveAtWaypoint(this);
        currentClaimers.Remove(npc);
        // If the NPC should not wait for this item to return
        // then just say we've arrived here and move on to the next
        // waypoint
        
        // remove everyone who was gonna go for this item
        foreach(var itemEntry in currentClaimers.Where(kvp => kvp.Value == item).ToList())
        {
            if (Random.Range(0f,1f) > chanceNPCWaitsForItem)
            {
                // stop waiting for item
                itemEntry.Key.ArriveAtWaypoint(this);
                currentClaimers.Remove(itemEntry.Key);
            }
        }
    }

    // Height for waypoint items will always be zero
    // unless we change this
    public float GetHeight()
    {
        return 0;
    }

    public IWaypointable[] GetFork()
    {
        IWaypointable[] fork = new IWaypointable[waypointFork.Length + itemWaypointFork.Length + jumpOffWaypointFork.Length];
        for (int i = 0; i < waypointFork.Length; i++)
        {
            fork[i] = waypointFork[i];
        }
        for (int i = 0; i < itemWaypointFork.Length; i++)
        {
            fork[waypointFork.Length + i] = itemWaypointFork[i];
        }
        for (int i = 0; i < jumpOffWaypointFork.Length; i++)
        {
            fork[waypointFork.Length + jumpOffWaypointFork.Length + i] = jumpOffWaypointFork[i];
        }
        return fork;
    }
}
