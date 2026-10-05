using System.Collections;
using UnityEngine;

public abstract class Movement : MonoBehaviour
{

    /*  enumeration of the possible ways the player can be moving */
    public enum Mode
    {
        //Walking,
        Running,
        Jetpacking,
        Gliding,
        Swimming,
        Biking,
        Wheeling,
        Horseback,
        Noclip,
        GetOffTheBoat,
        None,
    }

    // constants consist primarily of values for movement speeds
    protected const float rotationSpeed = 10f;
    protected const float walkSpeed = 2f;
    protected const float runSpeed = 5f;
    protected const float sprintSpeed = 7f;
    protected const float jetpackSpeed = 20f;
    protected const float swimSpeed = 3f;
    protected const float bikeSpeed = 10f;
    protected const float horseSpeed = 8.28852183406113537117903930131f;
    protected const float jumpForce = 300f;
    protected const float jetpackForce = 1500f;

    protected const float runAcceleration = 2.5f;
    protected const float swimAcceleration = 5f;
    protected const float bikeAcceleration = 1.2f;

    public float maxSpeed;
    public float acceleration;
    public float angularSpeed;
    public Transform itemDropPoint;
    public GameObject itemSpawnPrefab = null; // iteme that will be spawned if this movement mode is exited
    public float cooldownTimeAfterDismount = 1f;

    protected CameraController cameraController;

    protected Transform characterMesh;

    protected Animator anim;
    protected Rigidbody rb;
    protected Collider mainCollider;
    protected Racer racer;

    protected Vector3 velocity = Vector3.zero;
    protected Vector3 actualVelocity; // accounts for walking into walls
    protected Vector3 playerPosition; // position in previous frame

    // used to smooth out speed transition an animation
    protected float speed = 0;
    protected float smoothSpeed = 0;
    protected Vector3 smoothSpeedDirection;
    protected const float dampTime = 0.05f; // reduce jittering in animator by providing dampening

    protected float boostSpeedScale = 1f;
    protected float permanentSpeedScale = 1f;
    protected float physicalSpeedScale = 1f;

    // keep track of these because some movement modes will change these
    private float defaultMass;
    private float defaultDrag;
    private RigidbodyConstraints defaultConstraints;
    private float defaultAngularDrag;
    private Vector3 defaultCenterOfMass;
    private Vector3 defaultCharacterMeshPos;
    private Vector3 defaultCharacterMeshRot;

    public bool continueMotionAfterDeath = false;


    // used to determine when jumping can occur
    protected bool grounded = true;
    protected bool falling = false;
    protected bool launched = false;
    
    public Vector3 Velocity { get => actualVelocity; set => velocity = value; }
    public bool Falling { get => falling; set => falling = value; } 
    public bool Grounded { get => grounded; set => grounded = value; }
    public float BoostSpeedScale { get => boostSpeedScale; set => boostSpeedScale = value; }
    public float PermanentSpeedScale { get => permanentSpeedScale; set => permanentSpeedScale = value; }
    public float PhysicalSpeedScale { get => transform.localScale.x; }
    public CameraController CameraController { get => cameraController; set => cameraController = value; }
    public virtual Vector3 Forward { get => characterMesh.forward; }
    public virtual Vector3 ItemDropPoint { get => itemDropPoint.position; }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<CapsuleCollider>();
        racer = GetComponent<Racer>();

        characterMesh = transform.GetChild(0);

        anim = characterMesh.GetComponent<Animator>();
    }

    protected virtual void OnEnable()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        defaultMass = rb.mass;
        defaultDrag = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;
        defaultConstraints = rb.constraints;
        defaultCenterOfMass = rb.centerOfMass;

        characterMesh.transform.parent = transform;

        mainCollider.enabled = true;

        characterMesh = transform.GetChild(0);  // TODO change this, horse is gonna mess it up
        defaultCharacterMeshPos = characterMesh.localPosition;
        defaultCharacterMeshRot = characterMesh.localEulerAngles;
        playerPosition = transform.position;

        
    }

    protected virtual void OnDisable()
    {
        // Reset these to what they should be by default
        rb.mass = defaultMass;
        rb.linearDamping = defaultDrag;
        rb.angularDamping = defaultAngularDrag;
        rb.constraints = defaultConstraints;
        rb.centerOfMass = defaultCenterOfMass;
        characterMesh.localPosition = defaultCharacterMeshPos;
        characterMesh.localEulerAngles = defaultCharacterMeshRot;
        if (cameraController != null)
        {
            cameraController.ResetXMinMax();
        }
    }

    /*  rotate the camera around the player */
    public void RotateCamera(float x, float y)
    {
        cameraController.Rotate(x, y);
    }

    /*  moves the player rigidbody */
    public virtual void AddMovement(float forward, float up, float right)
    {
        if (Time.deltaTime > 0)
        {
            actualVelocity = Vector3.Lerp(actualVelocity, (transform.position - playerPosition) / Time.deltaTime, Time.deltaTime * 10);
        }
        playerPosition = transform.position;
    }

    /*  causes the player to jump */
    public virtual void Jump(bool hold)
    {
        racer.Revive();
    }

    public virtual void Launch(Vector3 force)
    {
        launched = true;
        rb.AddForce(force);
    }

    /*  grounds the player after a jump is complete */
    public virtual void Land()
    {
        grounded = true;
        if (launched)
        {
            launched = false;
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        
    }

    public virtual void Dismount()
    {
        // base level - do nothing
    }

    public virtual void StartSpeedBoost(float magnitude)
    {
        BoostSpeedScale = magnitude;
        anim.speed = BoostSpeedScale * PermanentSpeedScale;
    }
    
    public virtual void EndSpeedBoost()
    {
        BoostSpeedScale = 1f;
        anim.speed = PermanentSpeedScale;
    }

    public abstract void ApplyJumpSplosion(Vector3 force);

    protected virtual void LateUpdate()
    {

    }
}