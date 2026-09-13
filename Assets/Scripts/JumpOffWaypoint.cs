using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BabyBanjo.Polyathlon.Race
{
    [RequireComponent(typeof(BoxCollider))]
    public class JumpOffWaypoint : MonoBehaviour, IWaypointable
    {
        public IWaypointable Next { get { return next; } set { next = value; } }
        private IWaypointable next;
        public int Seq { get { return seq; } set { seq = value; } }
        private int seq;
        public Vector3 pos;
        public float height;
        public bool forceToGround = false;
        // If this is the end of the chain, you can add other chains for NPCs to decide to follow
        public Waypoint[] waypointFork; // Unity doesn't let you serialize interfaces :(
        public ItemWaypoint[] itemWaypointFork;
        public JumpOffWaypoint[] jumpOffWaypointFork;
        private Color color;
        private Dictionary<NPC, Vector3> npcDestinations;
        private BoxCollider boxCollider;

        private void Awake()
        {
            npcDestinations = new Dictionary<NPC, Vector3>();
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                pos = hit.position;
                color = new Color(0, 1, 0, 0.5f);
            }
            else
            {
                pos = transform.position;
                color = new Color(1, 0, 0, 0.5f);
            }
            boxCollider = GetComponent<BoxCollider>();
            if (!forceToGround)
                height = boxCollider.size.y / 2;
            else
                height = 0;
        }

        public Vector3 GetPos(NPC npc)
        {
            // return random position within box collider
            float x = Random.Range(pos.x + boxCollider.size.x / 2, pos.x - boxCollider.size.x / 2);
            float z = Random.Range(pos.z + boxCollider.size.z / 2, pos.z - boxCollider.size.z / 2);
            return new Vector3(x, pos.y, z);
        }

        public float GetHeight()
        {
            return height;
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

        private void OnTriggerStay(Collider other)
        {
            NPC npc = other.GetComponent<NPC>();
            if (npc != null && npc.movementMode == MovementMode.Gliding)    // check to make sure agent is part of this route
            {
                //Debug.Log(other);
                npc.ArriveAtWaypoint(this);
            }
        }

        private void OnDrawGizmos()
        {

            if (Application.isPlaying)
            {
                Gizmos.color = color;
                Gizmos.DrawSphere(pos, 1f);
                Gizmos.DrawLine(pos, pos + height * Vector3.up * 2);
                Gizmos.DrawSphere(pos + height * Vector3.up, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(transform.position, 1f);
                Gizmos.DrawLine(transform.position, transform.position + height * Vector3.up * 2);
                Gizmos.DrawSphere(transform.position + height * Vector3.up, 0.5f);
            }

        }


    }
}