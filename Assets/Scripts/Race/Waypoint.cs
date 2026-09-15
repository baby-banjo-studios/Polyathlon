using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Movement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BabyBanjo.Polyathlon.Race
{
    [RequireComponent(typeof(Collider))]
    public class Waypoint : MonoBehaviour, IWaypointable
    {
        public IWaypointable Next { get { return next; } set { next = value; } }
        private IWaypointable next;
        public int Seq { get { return seq; } set { seq = value; } }
        // If this is the end of the chain, you can add other chains for NPCs to decide to follow
        public Waypoint[] waypointFork; // Unity doesn't let you serialize interfaces :(
        public ItemWaypoint[] itemWaypointFork;
        public JumpOffWaypoint[] jumpOffWaypointFork;
        private int seq;
        public Vector3 pos;
        public float height;
        public bool water = false;
        public bool forceToGround = false;
        public MovementMode ignoreMovementMode = MovementMode.None;
        private Color color;
        private Dictionary<NPC, Vector3> npcDestinations;
        private Collider thisCollider;

        private void Awake()
        {
            thisCollider = transform.GetComponent<Collider>();
            npcDestinations = new Dictionary<NPC, Vector3>();
            if (!water) // place the position firmly on the ground
            {
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
            }
            else
            {
                pos = transform.position;
                color = new Color(1, 0, 0, 0.5f);
            }
            if (!forceToGround)
                if (thisCollider.GetType() == typeof(CapsuleCollider))
                {
                    height = Mathf.Max(((CapsuleCollider)thisCollider).radius, ((CapsuleCollider)thisCollider).height / 2) / 2;
                }
                else if (thisCollider.GetType() == typeof(BoxCollider))
                {
                    height = ((BoxCollider)thisCollider).size.y / 2;
                }
                else
                {
                    Debug.LogError("A Waypoint requires either a Box Collider or a Capsule Collider.");
                }
            else
                height = 0;
        }

        public Vector3 GetPos(NPC npc)
        {
            if (water)
            {
                if (!npcDestinations.ContainsKey(npc))
                {
                    if (thisCollider.GetType() == typeof(CapsuleCollider))
                    {
                        npcDestinations[npc] = pos + Random.insideUnitSphere * ((CapsuleCollider)thisCollider).radius * 0.9f;
                    }
                    else if (thisCollider.GetType() == typeof(BoxCollider))
                    {
                        Vector3 colliderSize = ((BoxCollider)thisCollider).size;
                        npcDestinations[npc] = pos + new Vector3(Random.Range(-colliderSize.x / 2, colliderSize.x / 2), 0, Random.Range(-colliderSize.z / 2, colliderSize.z / 2));
                    }
                    else
                    {
                        Debug.LogError("A Waypoint requires either a Box Collider or a Capsule Collider.");
                    }
                }
                return npcDestinations[npc];
            }
            else
                return pos;
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

        private void OnTriggerEnter(Collider other)
        {
            NPC npc = other.GetComponent<NPC>();
            if (npc != null)    // check to make sure agent is part of this route
            {
                //Debug.Log(other);
                if (npc.movementMode != ignoreMovementMode)
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