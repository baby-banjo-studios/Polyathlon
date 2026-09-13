using BabyBanjo.Polyathlon.Entities;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BabyBanjo.Polyathlon.Items
{
    public class PropelonObject : MelonObject
    {
        [SerializeField]
        protected Transform propellor;
        [SerializeField]
        protected float propellorRotRate = 360f;
        [SerializeField]
        protected AudioSource propellerAudioSource;

        [SerializeField] protected float selfRotRate = 30f;
        [SerializeField]
        protected float speed = 10f;
        [SerializeField]
        private float maxDistancePredict = 100;
        [SerializeField]
        private float minDistancePredict = 5;
        [SerializeField]
        protected float maxTimePrediction = 5f;
        protected Vector3 predictedPos;
        protected bool active = true;
        [SerializeField]
        protected float windDownTime = 5f;

        protected override void Awake()
        {
            base.Awake();
            if (propellor != null && propellerAudioSource == null)
            {
                propellerAudioSource = propellor.GetComponent<AudioSource>();
            }
        }

        protected override void Start()
        {
            base.Start();
            if (source != null)
            {
                speed = speed + source.linearVelocity.magnitude;
            }
        }

        private void Update()
        {
            if (active)
            {
                propellor.Rotate(propellorRotRate * Time.deltaTime, 0f, 0f);

                rb.linearVelocity = transform.forward * speed;

                if (target != null)
                {
                    float leadTimePercentage = Mathf.InverseLerp(minDistancePredict, maxDistancePredict, Vector3.Distance(transform.position, target.transform.position));
                    PredictMovement(leadTimePercentage);
                    RotateToTarget();
                }
            }
        }

        protected override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);

            if (active)
            {
                active = false;
                rb.useGravity = true;
                StartCoroutine(WindDownCoroutine());
            }
        }

        private void PredictMovement(float leadTimePercentage)
        {
            float predictionTime = Mathf.Lerp(0, maxTimePrediction, leadTimePercentage);
            predictedPos = target.position + (Vector3.up * 1.5f) + target.linearVelocity * predictionTime;
        }

        private void RotateToTarget()
        {
            Vector3 heading = predictedPos - transform.position;
            Quaternion rotation = Quaternion.LookRotation(heading);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, selfRotRate * Time.deltaTime));
        }

        private IEnumerator WindDownCoroutine()
        {
            float elapsedTime = 0f;
            float startingVol = audioSource.volume;
            while (elapsedTime <= windDownTime)
            {
                elapsedTime += Time.deltaTime;

                float rotRate = Mathf.Lerp(propellorRotRate, 0f, elapsedTime / windDownTime);
                propellor.Rotate(rotRate * Time.deltaTime, 0f, 0f);

                float volume = Mathf.Lerp(startingVol, 0f, elapsedTime / windDownTime);
                propellerAudioSource.volume = volume;

                yield return null;
            }
            propellerAudioSource.Stop();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, predictedPos);
            Gizmos.DrawSphere(predictedPos, 0.1f);
        }
    }
}