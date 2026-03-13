using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NPC : Racer
{
    protected WaypointChain chain;
    protected IWaypointable nextWaypoint;
    protected NavMeshAgent agent;

    protected const float jetpackVerticalForceOffset = 2f;
    protected const float jetpackHorizontalCorrection = 15f;

    protected const float agentSpeed = 5;

    private bool gliderControlActive = false;
    private bool isGoingToJetpack = false;

    protected override void Awake()
    {
        base.Awake();
        
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Start()
    {
        base.Start();

        GameObject chainObj = GameObject.FindWithTag("WaypointChainStart");
        if (chainObj != null)
        {
            chain = chainObj.GetComponent<WaypointChain>();
        }
        if (chain != null)
        {
            nextWaypoint = chain.GetStartingWaypoint();
        }
        SetNavMeshAgent(false);
        
        //agent.updatePosition = false;
        foreach(Movement m in movementOptions)
        {
            m.enabled = false;
            m.CameraController = null;
        }
        base.Start();
    }

    private Vector3 smoothedDesired;
    private const float desiredSmoothTime = 0.15f;


    protected override void Update()
    {
        
        base.Update();
        if (movementMode == Movement.Mode.Jetpacking)
        {
            if (transform.position.y < nextWaypoint.GetPos(this).y + nextWaypoint.GetHeight() - jetpackVerticalForceOffset)
            {
                SetNavMeshAgent(false);
                movement.Jump(true);
            }
            else if (!movement.Grounded)
            {
                movement.Jump(false);
            }
            if (!movement.Grounded)
            {
                Vector3 targetDir = nextWaypoint.GetPos(this) - transform.position - Vector3.Normalize(rb.linearVelocity) * jetpackHorizontalCorrection;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, 5f * Time.deltaTime, 0);
                newDir = new Vector3(newDir.x, 0, newDir.z);
                Debug.DrawRay(transform.position, newDir, Color.red);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
        else if (movementMode == Movement.Mode.Gliding)
        {
            if (!movement.Grounded)
            {
                if (!gliderControlActive)
                    StartCoroutine(GliderControl());
            }
        }
        else if (movementMode == Movement.Mode.Biking)
        {
            agent.acceleration = Mathf.Lerp(1.2f, 10f, rb.linearVelocity.magnitude / movement.maxSpeed);
        }
        else if (movementMode == Movement.Mode.Swimming || movementMode == Movement.Mode.GetOffTheBoat)
        {
            Vector3 targetDir = (nextWaypoint.GetPos(this) - transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, 5f * Time.deltaTime, 0);
            newDir = new Vector3(newDir.x, 0, newDir.z);
            Debug.DrawRay(transform.position, newDir, Color.red);
            transform.rotation = Quaternion.LookRotation(newDir);
            move = new Vector2(0, 1f);
        }
        else if (movementMode == Movement.Mode.Wheeling)
        {
            if (!agent.enabled || !agent.isOnNavMesh)
            {
                move = Vector2.zero;
                return;
            }

            // KEEP AGENT IN SYNC WITH PHYSICS
            agent.nextPosition = transform.position;

            Vector3 desired;

            if (agent.hasPath && agent.path.corners.Length > 1)
                desired = agent.path.corners[1] - transform.position;
            else
                desired = agent.desiredVelocity;

            if (desired.sqrMagnitude < 0.01f)
            {
                move = Vector2.zero;
                return;
            }

            // Smooth
            smoothedDesired = Vector3.Lerp(
                smoothedDesired,
                desired,
                Time.deltaTime * 6f
            );

            Vector3 flatDesired = Vector3.ProjectOnPlane(smoothedDesired, Vector3.up).normalized;

            float forwardIntent = Vector3.Dot(transform.forward, flatDesired);
            float turnIntent    = Vector3.Dot(transform.right,  flatDesired);

            forwardIntent = Mathf.Clamp(forwardIntent, -1f, 1f);
            turnIntent    = Mathf.Clamp(turnIntent, -1f, 1f);

            // Optional: slow down while turning
            forwardIntent *= Mathf.Lerp(1f, 0.4f, Mathf.Abs(turnIntent));

            // Match Wheeler axis expectations
            move.x = turnIntent;
            move.y = forwardIntent;
        }

    }

    void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            if (movementMode == Movement.Mode.Running || movementMode == Movement.Mode.Jetpacking && movement.Grounded)
            {
                if (movement.BonusSpeed == 1)
                    rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, 20);
            }
            else if (movementMode == Movement.Mode.Biking && movement.BonusSpeed == 1)
            {
                rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, 50);
            }
        }
    }


    IEnumerator GliderControl()
    {
        gliderControlActive = true;
        bool landOnHead = Random.Range(0, 2) == 1;
        float stallSpeed = Random.Range(15f, 25f);
        bool begunTurning = false;
        while(!movement.Grounded)
        {
            // Roll
            Vector3 nextWaypointPos = nextWaypoint.GetPos(this);
            Vector3 desiredDirection = new Vector3(nextWaypointPos.x, 0, nextWaypointPos.z) - new Vector3(transform.position.x, 0, transform.position.z);
            move = new Vector2(0, 0);
            // Don't let them start rolling too early or they won't have enough lift and they'll land prematurely
            if (begunTurning || rb.linearVelocity.magnitude > Mathf.Max(stallSpeed - 5f, 15f))
            {
                begunTurning = true;
                Vector3 velocityDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
                Vector3 cross = Vector3.Cross(velocityDirection, desiredDirection);
                // Target is to the right
                if (cross.y > 0 && (characterMesh.right.x > 0.3f || characterMesh.right.y > 0))
                {
                    move = new Vector2(0.1f, 0f);
                }
                // Target is to the left
                else if (cross.y < 0 && (characterMesh.right.x > 0.3f || characterMesh.right.y < 0))
                {
                    move = new Vector2(-0.1f, 0f);
                }
            }

            // Pitch
            float estimatedTimeToLanding = rb.linearVelocity.y / (transform.position.y - nextWaypointPos.y);
            Vector3 predictedLandingLocation = transform.position + (rb.linearVelocity * estimatedTimeToLanding);
            float distanceToLanding = Vector3.Distance(transform.position, predictedLandingLocation);
            float distanceToTarget = Vector3.Distance(transform.position, nextWaypointPos);

            // Also determine whether the target point is already behind us.
            // When targetAngle is positive, it's ahead, when negative, it's behind
            float targetAngle = Vector3.SignedAngle(desiredDirection, transform.forward, transform.right);

            // We passed it! Land immediately!
            if (targetAngle >= 90)
            {
                // Land on head
                if (landOnHead) {
                    if (characterMesh.up.z < 0.9f) {
                        move += new Vector2(0, 1f);
                    }
                } else {
                    // Land on feet
                    if (characterMesh.up.z > -0.9f) {
                        move += new Vector2(0, -1f);
                    }
                }
            }
            else
            {
                if (distanceToLanding < distanceToTarget)
                {
                    if (rb.linearVelocity.magnitude < stallSpeed && characterMesh.up.z < 0.5f)
                    {
                        move += new Vector2(0, 0.1f);
                    }
                    else if (rb.linearVelocity.magnitude > stallSpeed && characterMesh.up.z > -0.5f)
                    {
                        move += new Vector2(0, -0.1f);
                    }
                }
                else if (distanceToLanding > distanceToTarget && (characterMesh.up.z > -0.5f))
                {
                    
                    if (rb.linearVelocity.magnitude < stallSpeed && characterMesh.up.z > -0.5f)
                    {
                        move += new Vector2(0, -0.1f);
                    }
                    else if (rb.linearVelocity.magnitude > stallSpeed && characterMesh.up.z < 0.5f)
                    {
                        move += new Vector2(0, 0.1f);
                    }
                }
            }
            yield return null;
        } 
        move = new Vector2(0,0);
        gliderControlActive = false;
    }
    

    public override void EquipItem(Item item)
    {
        base.EquipItem(item);
        if (item != null)
        {
            StartCoroutine(UseItem());
        }
    }

    private IEnumerator UseItem()
    {
        float s = Random.Range(0.2f, 10f);
        yield return new WaitForSeconds(s);
        if (item != null)
            item.Use(this);
    }

    protected override IEnumerator SpeedBoostCoroutine(float magnitude)
    {
        movement.BonusSpeed = magnitude;
        anim.speed = magnitude;
        agent.speed = agentSpeed * magnitude;

        while (remainingBoostTime > 0)
        {
            remainingBoostTime -= Time.deltaTime;
            yield return null;
        }

        // reset everything
        movement.BonusSpeed = 1f;
        anim.speed = 1f;
        agent.speed = agentSpeed;
        remainingBoostTime = 0f;
        boostCoroutine = null;
    }

    /*  called by the RaceManager, makes the NPC start moving
        the NPC will not officially start the race until this is called */
    public override void StartRace()
    {
        base.StartRace();
        SetMovementMode(movementMode, true);
    }

    public void Land()
    {
        if (movementMode == Movement.Mode.Jetpacking || movementMode == Movement.Mode.Gliding)
        {
            SetNavMeshAgent(true);
            if (movementMode == Movement.Mode.Gliding)
            {
                characterMesh.localEulerAngles = new Vector3(0,0,0);
            }
        }
    }

    public override void Die(bool emphasizeTorso, Vector3 newMomentum = default)
    {
        base.Die(emphasizeTorso, newMomentum);
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    protected override IEnumerator RevivalEnabler()
    {
        // Don't allow a revival until we stop moving on the ground
        yield return new WaitUntil(() => !ragdoll.IsMoving());
        // then wait a bit before getting up...
        yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        canRevive = true;
        Revive();
    }

    public override void Revive(bool forceRevive = false)
    {
        base.Revive(forceRevive);
        if (agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    /*  updates player's movement mode and maxSpeed/locomotion accordingly */
    public override void SetMovementMode(Movement.Mode mode, bool initial = false)
    {
        Debug.Log("Setting mode: " + mode);
        base.SetMovementMode(mode, initial);
        move = new Vector2(0, 0);
        if (mode == Movement.Mode.Swimming || mode == Movement.Mode.GetOffTheBoat)
        {
            SetNavMeshAgent(false);
        }
        else
        {
            SetNavMeshAgent(true);
        }
        if (mode == Movement.Mode.Wheeling)
        {
            agent.updatePosition = false;
            // when the NPC drives the wheeler off a cliff, they get confused.
            StartCoroutine(FallMonitor());
            StartCoroutine(PathRefresher());
        }
        else
        {
            agent.updatePosition = true;
        }
        agent.speed = movement.maxSpeed;
        agent.acceleration = movement.acceleration;
        agent.angularSpeed = movement.angularSpeed;
    }

    public void ArriveAtWaypoint(IWaypointable waypoint)
    {
        if (((MonoBehaviour)waypoint).gameObject.CompareTag("FinalWaypoint"))
        {
            isGoingToJetpack = true;
        }
        if (waypoint.GetFork().Length == 0 && waypoint.Next == null)
        {
            if (agent.isOnNavMesh)
                agent.ResetPath();
        }
        else if (waypoint.GetFork().Length > 0)
        {
            nextWaypoint = waypoint.GetFork()[Random.Range(0, waypoint.GetFork().Length)];
        }
        else if (waypoint.Next != null)
        {
            nextWaypoint = waypoint.Next;
        }
        if (waypoint is JumpOffWaypoint)
        {
            SetNavMeshAgent(false);
            // Some NPCs should be smart and make beelines for target
            if (Random.Range(0,20) < 5)
            {
                Vector3 nextWaypointPos = nextWaypoint.GetPos(this);
                Vector3 targetDir = (new Vector3(nextWaypointPos.x, 0, nextWaypointPos.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, 4, 0);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
            movement.Jump(true);
        }
        else
        {
            if (agent.enabled && agent.isOnNavMesh)
                agent.SetDestination(nextWaypoint.GetPos(this));
        }
    }

    // When you end up not where you'd like to be, the best way out is a jetpack. Or the finish line.
    public void GoToNearestJetpack()
    {
        if (!isGoingToJetpack && !isFinished)
            StartCoroutine(GoToNearestJetpackOnceOnNavMesh());
    }

    private IEnumerator GoToNearestJetpackOnceOnNavMesh()
    {
        isGoingToJetpack = true;
        while(!(agent.enabled && agent.isOnNavMesh))
        {
            yield return null;
        }
        bool success = false;

        while (!success)
        {
            // TODO: Change this when upgrading Unity version later to FindObjectsByType
            var jetpackItems = Object.FindObjectsOfType(typeof(JetpackItem));
            Dictionary <float, IWaypointable> nearestJetpacks = new Dictionary<float, IWaypointable>();
            foreach (JetpackItem jetpackItem in jetpackItems)
            {
                NavMeshPath path = new NavMeshPath();
                if (GetPath(path, transform.position, jetpackItem.transform.position, NavMesh.AllAreas))
                {
                    float pathLength = GetPathLength(path);
                    if (pathLength >= 0 && !nearestJetpacks.Keys.Contains(pathLength))
                        nearestJetpacks.Add(pathLength, jetpackItem.transform.parent.GetComponent<IWaypointable>());
                }    
            }
            GameObject finishWaypoint = GameObject.FindGameObjectWithTag("FinalWaypoint");
            if (finishWaypoint != null)
            {
                NavMeshPath path = new NavMeshPath();
                if (GetPath(path, transform.position, finishWaypoint.transform.position, NavMesh.AllAreas))
                {
                    float pathLength = GetPathLength(path);
                    if (pathLength >= 0 && !nearestJetpacks.Keys.Contains(pathLength))
                        nearestJetpacks.Add(pathLength, finishWaypoint.GetComponent<IWaypointable>());
                }
            }
            if (nearestJetpacks.Keys.Count > 0)
            {
                float closestDistance = nearestJetpacks.Keys.Min();
                nextWaypoint = nearestJetpacks[closestDistance];
                while(!(agent.enabled && agent.isOnNavMesh))
                {
                    yield return null;
                }
                agent.SetDestination(nextWaypoint.GetPos(this));
                success = true;
            } else
            {
                yield return null;
            }
        }
        isGoingToJetpack = false;
    }

    // Interruptable coroutine!
    private IEnumerator FallMonitor()
    {
        float ypos = transform.position.y;
        while (movementMode == Movement.Mode.Wheeling)
        {
            yield return new WaitForSeconds(1);
            // if we fell more than 3 meters in one second
            if (ypos - transform.position.y > 3f)
            {
                ResetNavMeshAgent();
            }
        }
    }

    private IEnumerator PathRefresher()
    {
        while (movementMode == Movement.Mode.Wheeling)
        {
            yield return new WaitForSeconds(10 + Random.Range(-1f, 10f));
            // Every 9-20 seconds reset the path just to be safe
            // Vary the time so that it doesn't look like every NPC is refreshing at once
            ResetNavMeshAgent();
        }
    }

    private void ResetNavMeshAgent()
    {
        Vector3 currentDestination = agent.destination;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            // wait 1 frame to allow warp to complete so that we'll definitely be on a NavMesh
            StartCoroutine(ResumeAgentNextFrame(currentDestination));
        }
    }

    private IEnumerator ResumeAgentNextFrame(Vector3 destination)
    {
        yield return null; 

        if (!agent.isOnNavMesh)
            yield break;
        agent.ResetPath();
        agent.SetDestination(destination);
    }


    protected void SetNavMeshAgent(bool active)
    {
        agent.enabled = active;
        if (active && agent.isOnNavMesh && RaceManager.IsRaceActive)
        {
            agent.isStopped = false;
            agent.SetDestination(nextWaypoint.GetPos(this));
        }
    }

    // Credit to https://discussions.unity.com/t/getting-the-distance-in-nav-mesh/574818/6
    public static bool GetPath(NavMeshPath path, Vector3 fromPos, Vector3 toPos, int passableMask)
    {
        path.ClearCorners();
        if (NavMesh.CalculatePath(fromPos, toPos, passableMask, path) == false)
            return false;
       
        return true;
    }
       
    public static float GetPathLength(NavMeshPath path)
    {
        float pathLength = 0.0f;
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                pathLength += Vector3.Distance(path.corners[i-1], path.corners[i]);
            }
        }
        else
        {
            return -1f;
        }
        return pathLength;
    }
}