using BabyBanjo.Polyathlon.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.World
{
    public class LaserBolt : MonoBehaviour
    {
        public AudioClip laserImpact;
        public float impactVelMax = 30f;
        private Rigidbody rb;
        private float speed;
        private AudioSource audioSource;
        private Transform laserChild;
        private Racer owner;

        public void Initialize(float speed, Racer owner = null)
        {
            this.speed = speed;
            this.owner = owner;
        }

        private void Awake()
        {

        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = laserImpact;
            laserChild = transform.GetChild(0);
            rb = GetComponent<Rigidbody>();
            rb.linearVelocity = transform.forward * speed;
            StartCoroutine(DestroyIfMissed());
        }

        // Kill the racer if we hit them
        void OnTriggerEnter(Collider other)
        {
            Racer racer = other.gameObject.GetComponentInParent<Racer>();
            if (owner == null || (owner != null && racer != owner))
            {
                if (racer != null)
                {
                    racer.Die(true, Vector3.ClampMagnitude(rb.linearVelocity, impactVelMax));
                    Destroy(laserChild.gameObject);
                    rb.linearVelocity = Vector3.zero;
                    StartCoroutine(DestroyAfterPlayingSound());
                }
            }
            if ((owner == null || owner != racer) && !other.isTrigger)
            {
                Destroy(laserChild.gameObject);
                rb.linearVelocity = Vector3.zero;
                StartCoroutine(DestroyAfterPlayingSound());
            }
        }

        // Make sure the laser impact sound effect plays before we destory this
        private IEnumerator DestroyAfterPlayingSound()
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            Destroy(gameObject);
        }

        // Destroy the laser if we very clearly missed the target.
        private IEnumerator DestroyIfMissed()
        {
            yield return new WaitForSeconds(8);
            Destroy(gameObject);
        }


    }
}