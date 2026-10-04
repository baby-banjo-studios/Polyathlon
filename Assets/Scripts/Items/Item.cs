using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public float itemRespawnTime = 3;
    public bool consumable = false;         // for single-use (mystery box) items
    public bool dontRespawn = false;        // for items / equipment dropped by player (or one-time use equipment)
    protected bool pickupDisabled = false;  // for items that were just placed, to ensure player does not pick them up immediately after dropping


    public Sprite icon;
    public AudioClip soundWhenUsed;

    [SerializeField]
    private GameObject child;
    private Collider itemCollider;
    private ItemWaypoint itemWaypoint;

    protected bool available = true;

    public GameObject Child { get => child; }

    protected virtual void Start()
    {
        child = transform.GetChild(0).gameObject;
        itemCollider = GetComponent<Collider>();
    }

    // Called by an ItemWaypoint that is the parent of this gameObject
    public void AssignItemWaypoint(ItemWaypoint itemParent)
    {
        itemWaypoint = itemParent;
    }

    // Called when a racer picks up this item
    public virtual void Pickup(Racer racer)
    {
        if (consumable)
        {
            racer.EquipItem(this);
        }
        else
        {
            if (!dontRespawn)
            {
                StartCoroutine(RespawnItem());
            }
            // If the NPC has picked up this item...
            if (racer is NPC && itemWaypoint != null)
            {
                itemWaypoint.NPCTookItem((NPC)racer, this);
            }
        }
    }

    /* when the item is picked up, it should seem to disappear,
       then reappear after itemRespawnTime seconds */
    protected virtual IEnumerator RespawnItem()
    {
        // yield return null;
        // yield return null;
        available = false;
        child.SetActive(false);
        itemCollider.enabled = false;
        yield return new WaitForSeconds(itemRespawnTime);
        child.SetActive(true);
        itemCollider.enabled = true;
        available = true;
    }

    public bool IsAvailable()
    {
        return available;
    }

    public virtual void Use(Racer racer)
    {

    }

    public void Cooldown(float cooldownTime)
    {
        StartCoroutine(CooldownCoroutine(cooldownTime));
    }

    protected IEnumerator CooldownCoroutine(float cooldownTime)
    {
        pickupDisabled = true;
        yield return new WaitForSeconds(cooldownTime);
        pickupDisabled = false;
    }
}
