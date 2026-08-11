using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public enum TargetType
    {
        PlayersOnly,
        RacersOnly,
        AllEntities
    }

    private class TargetInfo
    {
        public TargetInfo(Entity target, float distance, bool visible)
        {
            this.target = target;
            this.distance = distance;
            this.visible = visible;
        }
        public Entity target;
        public float distance;
        public bool visible;
    }

    [SerializeField]
    private TargetType targetType;
    private SphereCollider triggerCollider;
    public float range;
    public LayerMask blockingMask;
    private List<TargetInfo> targetsInRange;

    private void Awake()
    {
        targetsInRange = new List<TargetInfo>();

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.radius = range;
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        
    }

    public Entity GetClosestVisibleTarget()
    {
        Entity currTarget = null;
        float shortestDistance = Mathf.Infinity;
        foreach (TargetInfo targetInfo in targetsInRange)
        {
            if (targetInfo.visible && targetInfo.distance < shortestDistance)
            {
                currTarget = targetInfo.target;
                shortestDistance = targetInfo.distance;
            }
        }
        return currTarget;
    }

    private void Update()
    {
        //
        foreach (TargetInfo targetInfo in targetsInRange)
        {
            targetInfo.visible = IsTargetVisible(targetInfo.target);
            targetInfo.distance = Vector3.Distance(transform.position, targetInfo.target.hips.position);
            Debug.DrawLine(transform.position, targetInfo.target.hips.position, targetInfo.visible ? Color.green : Color.red);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity target = FilterTargetTypeFromCollider(other);
        if (target != null)
        {
            TargetInfo newTarget = new TargetInfo(target, Vector3.Distance(transform.position, target.hips.position), IsTargetVisible(target));
            targetsInRange.Add(newTarget);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Entity target = FilterTargetTypeFromCollider(other);
        if (target != null)
        {
            foreach (TargetInfo existingTarget in targetsInRange)
            {
                if (existingTarget.target == target)
                {
                    targetsInRange.Remove(existingTarget);
                    break;
                }
            }
        }
    }

    private Entity FilterTargetTypeFromCollider(Collider other)
    {
        switch (targetType)
        {
            case TargetType.PlayersOnly:
                {
                    return other.GetComponent<PlayerController>();
                }
            case TargetType.RacersOnly:
                {
                    return other.GetComponent<Racer>();
                }
            case TargetType.AllEntities:
                {
                    return other.GetComponent<Entity>();
                }
        }
        return null;
    }

    private bool IsTargetVisible(Entity target)
    {
        if (Physics.Linecast(transform.position, target.hips.position, out RaycastHit hit, blockingMask.value))
        {
            if (hit.collider.gameObject == target.gameObject)
            {
                // no obstacle between source and target
                return true;
            }
            // hit something else
            return false;
        }
        // no collision - can happen if we dont count the target's layermask
        return true;
    }
}