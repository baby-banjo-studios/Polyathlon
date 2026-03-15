using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Wheeler : Movement
{
    
 
    public float driveForce = 800f;

    public GameObject wheeler;


    private float forward;
    private float right;
    private Vector3 defaultCOM;
    private Rigidbody leftWheel;
    private Rigidbody rightWheel;

    void Start()
    {


       leftWheel = transform.Find("Rideables/Wheeler/Wheeler Structure/Left Wheel").GetComponent<Rigidbody>();
       leftWheel.GetComponent<ConfigurableJoint>().connectedBody = rb;
       leftWheel.solverIterations = 30;
       rightWheel = transform.Find("Rideables/Wheeler/Wheeler Structure/Right Wheel").GetComponent<Rigidbody>();
       rightWheel.GetComponent<ConfigurableJoint>().connectedBody = rb;
       rightWheel.solverIterations = 30;
    }

    // enable wheeler
    protected override void OnEnable() 
    {
        base.OnEnable();

        SetWheeler(true);
    }

    // Put away wheeler
    protected override void OnDisable()
    {
        base.OnDisable();
        SetWheeler(false);
    }

    // Simple non-camera relative steering, less jittery but less intuitive
    /*  
    public override void AddMovement(float forward, float right)
    {
        base.AddMovement(forward, right);
        // they're flipped here for some reason
        this.right = forward;
        this.forward = right;
    }*/

    // Calculate camera-relative steering.
    public override void AddMovement(float inputForward, float inputRight)
    {
        base.AddMovement(inputForward, inputRight);

        float rawForward = inputRight;   // W/S
        float rawTurn    = inputForward; // A/D

        if (cameraController == null)
        {
            forward = rawForward;
            right   = rawTurn;
            return;
        }

        Vector3 toCamera = cameraController.cameraTransform.position - transform.position;
        toCamera = Vector3.ProjectOnPlane(toCamera, Vector3.up).normalized;

        Vector3 desiredForward = -toCamera;

        float yawError = Vector3.SignedAngle(
            transform.forward,
            desiredForward,
            Vector3.up
        );

        // Dead Zone
        const float yawDeadZone = 2f;
        if (Mathf.Abs(yawError) < yawDeadZone)
            yawError = 0f;

        // Camera align turn
        float alignTurn = 0f;
        if (rawForward != 0 || rawTurn != 0)
        {
            if (yawError != 0f && Mathf.Abs(rawForward) > 0.01f)
            {
                alignTurn = Mathf.Sign(yawError) * Mathf.Min(Mathf.Abs(yawError) / 45f, 1f);
            }
            right = Mathf.Clamp(rawTurn + alignTurn, -1f, 1f);
        }
        else
        {
            right = 0;
        }
        forward = rawForward;
    }


    float GetPitchAngle()
    {
        Vector3 forward = transform.forward;
        Vector3 flatForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        return Vector3.SignedAngle(flatForward, forward, transform.right);
    }

    void FixedUpdate()
    {
        ApplyDriveForce();        // translation
        ApplyTurnForce();              // yaw
        ApplyBalanceTorque();       // self-righting

        // Clamp velocity
        
        Vector3 horizontalVel = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);

        if (horizontalVel.magnitude > maxSpeed * BonusSpeed)
        {
            Vector3 clampedHorizontal = horizontalVel.normalized * maxSpeed * BonusSpeed;
            rb.linearVelocity = clampedHorizontal + Vector3.up * rb.linearVelocity.y;
        }


        // clamp drift
        float maxDriftSpeed = 1;
        float driftSpeed = Vector3.Dot(rb.linearVelocity, transform.right);

        driftSpeed = Mathf.Clamp(driftSpeed, -maxDriftSpeed, maxDriftSpeed);

        Vector3 lateral = rb.linearVelocity - transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = transform.right * driftSpeed + lateral;


    }


    bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            -transform.up,
            out _,
            1.0f
        );
    }


    void ApplyDriveForce()
    {
        if (forward != 0)
        {
            if (!IsGrounded())
                return;

            Vector3 pitchAxis =
                Vector3.Cross(transform.forward, Vector3.up).normalized;

            Vector3 driveDir =
                - Vector3.Cross(pitchAxis, Vector3.up).normalized;

            rb.AddForce(
                driveDir * forward * driveForce * BonusSpeed,
                ForceMode.Force
            );
        }
    }

    void ApplyBalanceTorque()
    {
        // Calculate the rotation needed to get from our current 'Up' to World 'Up'
        Quaternion uprightTarget = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        Quaternion deltaRotation = uprightTarget * Quaternion.Inverse(transform.rotation);

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        // If the angle is negligible, don't apply force to avoid micro-vibrations
        if (float.IsNaN(axis.x) || angle < 0.1f) return;

        // Adjust these values to change how "stiff" the wheeler feels
        float balanceStrength = 50f; // How hard it pulls back
        float balanceDamping = 10f;   // Prevents it from oscillating (overshooting)

        // Apply torque. Acceleration mode ignores the mass and keeps it consistent.
        Vector3 torque = axis.normalized * (angle * balanceStrength) - (rb.angularVelocity * balanceDamping);
        
        // We only want to affect pitch and roll, leave yaw for turning ApplyTurnForce
        torque.y = 0; 

        rb.AddTorque(torque, ForceMode.Acceleration);
    }


    void ApplyTurnForce()
    {
        float turnSpeed = 200f; 

        if (right != 0)
        {
            // Use transform.up so it rotates around its own local vertical axis 
            // even when tilted
            Vector3 targetTurn = transform.up * (right * turnSpeed * Mathf.Deg2Rad);
            rb.angularVelocity = new Vector3(targetTurn.x, targetTurn.y, targetTurn.z);
        }
        else
        {
            // Damping logic remains the same
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 15f);
        }
    }

    public override void Jump(bool hold)
    {
        base.Jump(hold);
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        Jump(true);
        Launch(force * rb.mass * 4 + rb.linearVelocity.normalized * 15);
    }

    public virtual void SetWheeler(bool enabled)
    {
        wheeler.SetActive(enabled);

        if (enabled)
        {
            characterMesh.localPosition = new Vector3(0,0.56f,0);
            rb.mass = 100;
            rb.linearVelocity = new Vector3(0,0,0);
            rb.angularDamping = 5.0f;
            defaultCOM = rb.centerOfMass;
            rb.centerOfMass = new Vector3(0f, -3f, 0f);
            rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            ConfigurableJoint[] joints = wheeler.GetComponentsInChildren<ConfigurableJoint>();
            if (cameraController != null)
                cameraController.EnableYawDecoupling();
        }
        else
        {
            characterMesh.localPosition = new Vector3(0,0,0);
            characterMesh.localEulerAngles = new Vector3(0,0,0);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            rb.mass = 1;
            rb.angularDamping = 0.05f;
            rb.centerOfMass = defaultCOM;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.None;
            if (cameraController != null)
                cameraController.DisableYawDecoupling();
        }
    }
}
