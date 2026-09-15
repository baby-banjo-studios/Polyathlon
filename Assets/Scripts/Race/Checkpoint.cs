using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Race
{
    [RequireComponent(typeof(BoxCollider))]
    public class Checkpoint : MonoBehaviour
    {
        public int seq;
        public Checkpoint next;
        public float distance = 0f;

        private BoxCollider box;

        private void OnTriggerEnter(Collider other)
        {
            Racer racer = other.GetComponent<Racer>();
            if (racer != null)    // check to make sure agent is part of this route
            {
                //Debug.Log(other);
                racer.ArriveAtCheckpoint(this);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(Vector3.zero, GetComponent<BoxCollider>().size);
        }
    }
}