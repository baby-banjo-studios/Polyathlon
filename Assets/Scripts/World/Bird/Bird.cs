using BabyBanjo.Polyathlon.Entities;
using UnityEngine;
using System.Collections;

namespace BabyBanjo.Polyathlon.World
{
    [System.Serializable]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(AudioSource))]
    public class Bird : MonoBehaviour
    {
        public float speed = 10f;           // units per second;
        public float rotationSpeed = 360f;  // degrees per second

        public AudioClip[] birdSounds;

        private Transform birdCircle;
        public bool circle = true;          // if true, bird is circling

        private Vector3 startPos;
        private Quaternion startRot;
        private Racer target;               // racer currently being targeted
        private Rigidbody rb;
        private AudioSource audioSource;

        private const float maxDist = 50f;  // max distance before we give up on the racer

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
            startPos = transform.localPosition;
            startRot = transform.localRotation;
            birdCircle = transform.parent;
        }

        /*  if a racer gets too close to this bird, make it a target */
        private void OnTriggerEnter(Collider other)
        {
            Racer racer = other.transform.GetComponent<Racer>();
            if (racer != null)
            {
                SetTarget(racer);
            }
        }

        /*  sets the bird's target to be the racer that just got too close to the circle */
        public void SetTarget(Racer racer)
        {
            if (target == null)
            {
                target = racer;
                circle = false;
            }
            StartCoroutine(ChaseTarget());
        }

        /*  Kill the target if we hit them */
        private void OnCollisionEnter(Collision other)
        {
            Racer racer = other.gameObject.GetComponent<Racer>();
            if (racer != null)
            {
                racer.Die(true, Vector3.ClampMagnitude(rb.linearVelocity, 20f));
            }
            target = null;
        }

        /*  the bird will follow its target unitil they collide or target is too far */
        private IEnumerator ChaseTarget()
        {
            PlayBirdSound();
            StartCoroutine(DelayBirdSound());

            while (target != null)
            {
                // look at target with rotation limit
                Vector3 targetDir = (target.transform.position - transform.position).normalized;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotationSpeed * Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(newDir);

                rb.linearVelocity = transform.forward * speed;

                // if target is too far away, give up
                if (Vector3.Distance(transform.position, target.transform.position) > maxDist)
                {
                    target = null;
                }
                yield return null;
            }
            PlayBirdSound();
            StartCoroutine(ReturnToCircle());
        }

        /*  returns the bird to its starting position in its circle 
            this will be interrupted if the bird finds a new target first */
        private IEnumerator ReturnToCircle()
        {
            // go back to center of circle
            while (target == null && Vector3.Distance(birdCircle.position, transform.position) > 0.5)
            {
                // look at circle position with a rotation limit
                Vector3 targetDir = (birdCircle.transform.position - transform.position).normalized;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotationSpeed * Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(newDir);

                rb.linearVelocity = transform.forward * speed;
                yield return null;
            }
            // reset rigidbody stuff
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // lerp to the EXACT correct positon
            float elapsedTime = 0f;
            float duration = 1f;
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            while (elapsedTime < 1f)
            {
                transform.localPosition = Vector3.Lerp(pos, startPos, elapsedTime / duration);
                transform.localRotation = Quaternion.Lerp(rot, startRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            circle = true;
        }

        /*  plays a random sound that the bird can make */
        private void PlayBirdSound()
        {
            if (birdSounds.Length != 0)
            {
                int i = Random.Range(0, birdSounds.Length);
                {
                    audioSource.clip = birdSounds[i];
                    audioSource.Play();
                }
            }
        }

        /*  plays a bird sound after a random interval */
        private IEnumerator DelayBirdSound()
        {
            while (!circle)
            {
                float delay = Random.Range(2f, 4f);
                yield return new WaitForSeconds(delay);
                if (!circle)
                {
                    PlayBirdSound();
                    // StartCoroutine(DelayBirdSound());   // is this ok? C# has tail recursion, right? // Update: it does not
                }
            }
        }
    }
}