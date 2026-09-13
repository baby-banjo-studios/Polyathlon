using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.World
{
    public class FloatingPlatform : MonoBehaviour
    {
        const float laserSpeed = 120f;
        const float propSpinSpeed = 500f;
        const float aimSpeed = 6f;
        float floatAmplitude;
        float floatSpeed;

        public LaserCannon cannon;
        public Rigidbody[] propellers;
        private Rigidbody rb;
        private bool activated = false;

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>();

            // Set floatAmplitude and floatSpeed randomly so that each platform has unique motion
            floatAmplitude = Random.Range(1, 2.5f);
            floatSpeed = Random.Range(1, 3);

            // Start spinning the propellers and controlling the cannon
            StartCoroutine(Hover());

            // Only aim the cannon if it is active
            // (just set it as inactive if you wanna use this platform for peaceful purposes)
            if (cannon.gameObject.activeSelf)
            {
                cannon.AimAndShoot(-1);
            }
        }

        // Handle propeller spinning and vertical oscillation
        private IEnumerator Hover()
        {
            while (true)
            {
                // oscillate vertically
                rb.MovePosition(rb.position + Vector3.up * Mathf.Cos(Time.time * floatSpeed) * Time.fixedDeltaTime * floatAmplitude);
                // spin the propellers
                foreach (Rigidbody prop in propellers)
                {
                    prop.MoveRotation(Quaternion.Euler(prop.rotation.eulerAngles.x, prop.rotation.eulerAngles.y + propSpinSpeed * Time.fixedDeltaTime, prop.rotation.eulerAngles.z));
                    prop.MovePosition(prop.position + Vector3.up * Mathf.Cos(Time.time * floatSpeed) * Time.fixedDeltaTime * floatAmplitude);
                }
                yield return null;
            }
        }
    }
}