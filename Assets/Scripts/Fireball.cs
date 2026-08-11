using UnityEngine;

public class Fireball : LaserBolt
{
    private ParticleSystem explosionPS;
    

    public override void Initialize(float speed, Entity owner = null)
    {
        base.Initialize(speed, owner);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    // Kill the racer if we hit them
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
}