using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Race;
using UnityEngine;
using System.Collections;

namespace BabyBanjo.Polyathlon.Items
{
    public class Item : MonoBehaviour
    {
        public float itemRespawnTime = 3;
        public bool consumable = false;

        public Sprite icon;
        public AudioClip soundWhenUsed;

        [SerializeField]
        private GameObject child;
        private Collider itemCollider;
        private ItemWaypoint itemWaypoint;

        private bool available = true;

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
                StartCoroutine(RespawnItem());
                // If the NPC has picked up this item...
                if (racer is NPC && itemWaypoint != null)
                {
                    itemWaypoint.NPCTookItem((NPC)racer, this);
                }
            }
        }

        /* when the item is picked up, it should seem to disappear,
        then reappear after itemRespawnTime seconds */
        protected IEnumerator RespawnItem()
        {
            yield return null;
            yield return null;
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
    }
}