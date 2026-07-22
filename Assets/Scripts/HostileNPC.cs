using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class HostileNPC : Entity
{    

    [SerializeField]
    protected float engageDistance = 25f;
    [SerializeField]
    protected float maxRotationPerSecond_deg = 90f;
    protected Sensor sensor;
    public Entity target = null;

    protected override void Awake()
    {
        base.Awake();
        sensor = GetComponentInChildren<Sensor>();
    }

    protected override void Update()
    {
        base.Update();
        if (sensor != null)
        {
            target = sensor.GetClosestVisibleTarget();
        }

        if (target != null)
        {
            // get direction to target
            Vector3 dirToTarget = target.transform.position - transform.position;
            Quaternion rotToTarget = Quaternion.LookRotation(dirToTarget);
            Quaternion targetRotation = Quaternion.Euler(0f, rotToTarget.eulerAngles.y, 0f);

            // rotate to face target
            float maxRotationThisFrame = maxRotationPerSecond_deg * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationThisFrame);
        }
    }
}