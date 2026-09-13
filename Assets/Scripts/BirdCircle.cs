using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.World
{
    public class BirdCircle : MonoBehaviour
    {
        private Bird bird;
        private float circleSpeed;  // rotations per second 
        public bool reverse = false;
        public float percentChanceBirdGoesAfterNPC = 50f;

        private void Start()
        {
            bird = transform.GetChild(0).GetComponent<Bird>();
            float r = GetComponent<CapsuleCollider>().radius;
            bird.transform.localPosition = new Vector3(r, 0, 0);
            bird.transform.localEulerAngles = new Vector3(0, reverse ? 180 : 0, 0);
            // calculate angular speed required to make bird go at its speed   
            circleSpeed = (reverse ? -1 : 1) * (180f / Mathf.PI) * (bird.speed / r);
        }

        private void Update()
        {
            if (bird.circle)
                transform.Rotate(0f, -1 * circleSpeed * Time.deltaTime, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            Racer racer = other.transform.GetComponent<Racer>();
            if (racer != null)
            {
                bool attack = true;
                if (racer is NPC)
                {
                    attack = Random.Range(0, 100) < percentChanceBirdGoesAfterNPC;
                }
                bird.SetTarget(racer);
            }
        }

        private void OnDrawGizmos()
        {
            float r = GetComponent<CapsuleCollider>().radius;
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Gizmos.DrawSphere(transform.position, r);
        }
    }
}