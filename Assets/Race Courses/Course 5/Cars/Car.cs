using UnityEngine;
using EasyRoads3Dv3; // Requires EasyRoads3D Pro

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    public AudioClip[] impactSounds;
    public AudioClip[] horn;
    private string targetRoadName;
    private float speed;
    private float rotationSpeed;
    private float reachDistance;

    private Vector3[] centerPoints;
    private Vector3[] leftPoints;
    private Vector3[] rightPoints;

    // -1.0 (Left edge) | 0.0 (Center) | +1.0 (Right edge)
    private float normalizedLaneOffset = 0f; 

    private int currentIndex = 0;
    private int stepDirection = 1; // +1 = forward along spline, -1 = reverse
    private bool isInitialized = false;

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Ensure interpolation is enabled for smooth visual movement at low physics tick rates
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        LaneSettings laneSettings = GetComponentInParent<LaneSettings>();
        if (laneSettings != null)
        {
            targetRoadName = laneSettings.targetRoadName;
            speed = laneSettings.speed;
            rotationSpeed = laneSettings.rotationSpeed;
            reachDistance = laneSettings.reachDistance;
        } else
        {
            Debug.LogError("Parent of " + gameObject.name + " must contain LaneSettings.");
        }
        InitializeRoadPath();
    }

    void InitializeRoadPath()
    {
        ERRoadNetwork roadNetwork = new ERRoadNetwork();
        ERRoad road = roadNetwork.GetRoadByName(targetRoadName);

        if (road == null)
        {
            Debug.LogError($"Road '{targetRoadName}' not found!");
            return;
        }

        // 1. Fetch pre-sampled splines
        centerPoints = road.GetSplinePointsCenter();
        leftPoints = road.GetSplinePointsLeftSide();
        rightPoints = road.GetSplinePointsRightSide();

        if (centerPoints == null || centerPoints.Length < 2) return;

        // 2. Find nearest waypoint to starting position
        currentIndex = GetClosestWaypointIndex(centerPoints);

        // 3. Determine travel direction relative to node array
        int nextIndex = (currentIndex + 1) % centerPoints.Length;
        Vector3 forwardDir = (centerPoints[nextIndex] - centerPoints[currentIndex]).normalized;

        bool isFacingForward = Vector3.Dot(transform.forward, forwardDir) >= 0;
        stepDirection = isFacingForward ? 1 : -1;

        // 4. Calculate initial lane offset ratio (-1.0 to +1.0) relative to dynamic road width
        Vector3 nodeLeft = leftPoints[currentIndex];
        Vector3 nodeRight = rightPoints[currentIndex];
        
        float currentDynamicWidth = Vector3.Distance(nodeLeft, nodeRight);
        Vector3 roadRight = (nodeRight - nodeLeft).normalized;

        Vector3 carToCenter = transform.position - centerPoints[currentIndex];
        float distanceOffset = Vector3.Dot(carToCenter, roadRight);

        float halfWidth = currentDynamicWidth * 0.5f;
        normalizedLaneOffset = Mathf.Clamp(distanceOffset / halfWidth, -1.0f, 1.0f);

        isInitialized = true;
    }

    int GetClosestWaypointIndex(Vector3[] points)
    {
        int closestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < points.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, points[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    void FixedUpdate()
    {
        if (!isInitialized || centerPoints == null || centerPoints.Length == 0)
            return;

        // Fetch full 3D target position for elevation support
        Vector3 moveTarget = GetLaneTargetPoint(currentIndex);

        Vector3 vectorToTarget = moveTarget - rb.position;
        float distanceToTarget = vectorToTarget.magnitude;

        // 1. DYNAMIC TURN SPEED
        float effectiveTurnSpeed = Mathf.Max(rotationSpeed, (speed / 3f));

        // 2. PHYSICS ROTATION
        Vector3 direction = vectorToTarget.normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, effectiveTurnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }

        // 3. PHYSICS MOVEMENT
        Vector3 nextPosition = rb.position + (transform.forward * speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);

        // 4. ADVANCE CONDITIONS
        bool reached = distanceToTarget <= reachDistance;

        // 3D Plane pass-by check to prevent orbiting
        int nextIdx = GetNextIndex(currentIndex);
        Vector3 segmentForward = (GetLaneTargetPoint(nextIdx) - moveTarget).normalized;
        bool passedNodePlane = Vector3.Dot(rb.position - moveTarget, segmentForward) > 0f;

        if (reached || passedNodePlane)
        {
            AdvanceToNextNode();
        }
    }

    Vector3 GetLaneTargetPoint(int index)
    {
        if (normalizedLaneOffset >= 0f)
        {
            return Vector3.Lerp(centerPoints[index], rightPoints[index], normalizedLaneOffset);
        }
        else
        {
            return Vector3.Lerp(centerPoints[index], leftPoints[index], Mathf.Abs(normalizedLaneOffset));
        }
    }

    void AdvanceToNextNode()
    {
        currentIndex = GetNextIndex(currentIndex);
    }

    int GetNextIndex(int index)
    {
        int next = index + stepDirection;
        if (next >= centerPoints.Length) return 0;
        if (next < 0) return centerPoints.Length - 1;
        return next;
    }

    void OnCollisionEnter(Collision other)
    {
        Racer racer = other.gameObject.GetComponent<Racer>();
        if (racer != null)
        {
            racer.Die(true);
            // play them at once to create a bigger impact
            foreach(AudioClip impactSound in impactSounds)
            {
                audioSource.PlayOneShot(impactSound);
            }
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Honk at racers who are in the way
        if (other.gameObject.layer == LayerMask.NameToLayer("Racer"))
        {
            // Pick a random clip
            AudioClip clip = horn[Random.Range(0, horn.Length)];

            // Store original pitch so we don't permanently mess up other sounds
            float originalPitch = audioSource.pitch;

            // Randomize pitch: e.g., 0.7f (lower/deeper/slower)
            audioSource.pitch = Random.Range(0.4f, 1f);

            // Play the honk with the modified pitch
            audioSource.PlayOneShot(clip);

            // Reset pitch back to normal (1.0)
            audioSource.pitch = originalPitch;
        }
    }

    void OnDrawGizmos()
    {
        if (!isInitialized || centerPoints == null || centerPoints.Length == 0) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(GetLaneTargetPoint(currentIndex), 0.5f);
    }
}