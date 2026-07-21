using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class HostileNPC : Entity
{    

    protected Racer target = null;
    [SerializeField]
    protected float engageDistance = 25f;
    protected Sensor sensor;
    public Entity target2;

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
            Entity target = sensor.GetClosestVisibleTarget();
            target2 = target;
        }
    }
}