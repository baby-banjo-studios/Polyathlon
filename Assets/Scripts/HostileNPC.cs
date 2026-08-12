using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;

public class HostileNPC : NPC
{    

    // public enum AttackMode
    // {
    //     Melee,
    //     Ranged
    // }

    // public AttackMode attackMode;

    [SerializeField]
    protected float minEngageDistance = 25f, nominalEngageDistance = 25f, maxEngageDistance = 25f;
    protected Vector3 navigationTargetPosition;

    [SerializeField]
    protected float maxRotationPerSecond_deg = 90f;
    protected Sensor sensor;
    public Entity target = null;
    public float timeBetweenAttacks = 10f;
    private float countdownToAttack;

    // melee attack fields
    [SerializeField]
    protected MeleeWeapon meleeWeapon;

    // ranged attack fields
    [SerializeField]
    protected Transform projectileOrigin;
    [SerializeField]
    protected GameObject projectilePrefab;
    [SerializeField]
    protected float projectileSpeed = 20f;


    protected override void Awake()
    {
        base.Awake();

        // sanity check
        if (minEngageDistance > nominalEngageDistance || nominalEngageDistance > maxEngageDistance)
        {
            Debug.LogError(String.Format("Hostile NPC {0} : engage distances {1},{2},{3} are invalid", gameObject.name, minEngageDistance, nominalEngageDistance, maxEngageDistance));
        }

        sensor = GetComponentInChildren<Sensor>();
        agent = GetComponent<NavMeshAgent>();

        countdownToAttack = timeBetweenAttacks;
        navigationTargetPosition = transform.position;
    }
    protected override void Start()
    {
        base.Start();
        if (meleeWeapon != null)
        {
            meleeWeapon.Initialize(this);
        }   
    }

    protected override void Update()
    {
        base.Update();
        
        // if (!agent.pathPending)
        // {
        //     if (agent.remainingDistance <= agent.stoppingDistance)
        //     {
        //         if (agent.)
        //     }
        // }
        
        if (!dead && RaceManager.IsRaceActive && !RaceManager.IsPaused)
        {
            if (sensor != null)
            {
                target = sensor.GetClosestVisibleTarget();
            }

            if (target != null)
            {
                // get direction to target
                Vector3 dirToTarget = target.transform.position - transform.position;
                float distToTarget = dirToTarget.magnitude;

                if (distToTarget > maxEngageDistance || distToTarget < minEngageDistance)
                {
                    // move closer to or farther away from  target
                    Vector3 nominalPointOnLine = target.transform.position - dirToTarget.normalized * nominalEngageDistance;
                    if (NavMesh.SamplePosition(nominalPointOnLine, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                    {
                        Debug.DrawLine(transform.position, navigationTargetPosition, Color.aliceBlue);
                        navigationTargetPosition = hit.position;
                        agent.SetDestination(navigationTargetPosition);
                    }
                    
                }
                // else if (distToTarget < minEngageDistance)
                // {
                //     // move away from target
                //     Vector3 nominalPointOnLine = target.transform.position - dirToTarget * nominalEngageDistance;
                // }
                else
                {
                    // good distance to target - face target and attack
                    Quaternion rotToTarget = Quaternion.LookRotation(dirToTarget);
                    Quaternion targetRotation = Quaternion.Euler(0f, rotToTarget.eulerAngles.y, 0f);

                    // rotate to face target
                    float maxRotationThisFrame = maxRotationPerSecond_deg * Time.deltaTime;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationThisFrame);

                    countdownToAttack -= Time.deltaTime;
                    if (countdownToAttack <= 0)
                    {
                        countdownToAttack = timeBetweenAttacks;
                        Attack();
                    }
                }
            }
            else
            {
                countdownToAttack = timeBetweenAttacks;
            }
        }
    }

    protected virtual void Attack()
    {
        // callbacks for actual attack method are baked into animation
        anim.SetTrigger("attack");
    }

    // will be called by animation
    public void SpawnProjectile()
    {
        if (projectileOrigin != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileOrigin.transform.position, transform.rotation);
            if (projectile.TryGetComponent(out LaserBolt lb))
            {
                lb.Initialize(projectileSpeed, this);
            }
        }
    }

    public void SetAttacking(bool value)
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.Arm(value);
        }
    }
}