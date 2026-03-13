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


    void ApplyTurnForce()
    {
        // Turn using an offset force so that we can keep the center of mass where it is without
        // causing the wheeler to rotate over the local Z axis
        if (right != 0)
        {
            
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 force = -flatForward * 10;
            Vector3 yawRight = Vector3.Cross(Vector3.up, flatForward); // right, yaw-only
            Vector3 posRight = transform.position + yawRight * 10f * right;
            Vector3 posLeft = transform.position - yawRight * 10f * right; // left, yaw-only
            rb.AddForceAtPosition(force, posRight, ForceMode.Impulse);
            rb.AddForceAtPosition(-force, posLeft, ForceMode.Impulse);
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
