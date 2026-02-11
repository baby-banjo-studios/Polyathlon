using UnityEngine;
using System.Collections;

public class Racer : MonoBehaviour
{
    public new string name;
    

    public Movement.Mode movementMode;
    public Movement.Mode prevMovementMode; // sometimes its helpful to know what we had previously
    public Movement[] movementOptions;

    public Transform characterMesh;
    public Transform hips;

    protected Item item;
    
    protected AnimatorOverrideController animOverride;
    protected PlayerAnimationEvents animEvents;

    protected Movement movement;
    protected Ragdoll ragdoll;
    protected BackpackMount backpackMount;
    protected Rigidbody rb;
    protected Animator anim;
    protected AudioSource audioSource;
    
    protected Vector2 move;
    protected Vector3 velocityBeforePhysicsUpdate;
    protected bool dead;
    protected bool canRevive; // when this is true, a dead racer can be revived.

    public int place;
    public float dieThreshold = 40f;
    public Checkpoint lastCheckpoint;
    public Checkpoint nextCheckpoint;
    public bool isFinished = false;

    [Header("Sound Effects")]
    public AudioClip bikeSound;
    public AudioClip waterSound;
    public AudioClip equipSound;

    public BackpackMount BackpackMount { get => backpackMount; }
    public Vector3 Forward { get => movement.Forward; }
    public Vector3 ItemDropPoint { get => movement.ItemDropPoint; }

    public float Speed
    {
        get
        {
            if (ragdoll.IsEnabled)
            {
                return ragdoll.Speed;
            }
            return rb.linearVelocity.magnitude;
        }
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ragdoll = GetComponentInChildren<Ragdoll>();
        anim = characterMesh.GetComponent<Animator>();
        animEvents = GetComponentInChildren<PlayerAnimationEvents>();

        //animOverride = GetComponent<AnimatorOverrideController>();
        audioSource = GetComponentInChildren<AudioSource>();
        backpackMount = GetComponentInChildren<BackpackMount>();
    }

    protected virtual void Start() 
    {
        ragdoll.SetRagdoll(false);
        SetMovementMode(movementMode, true);
    } 

    protected virtual void Update()
    {
        if (!dead && RaceManager.IsRaceActive && !RaceManager.IsPaused)
        {
            movement.AddMovement(move.x, move.y);
        }

        Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 3f, Color.green);
    }

    public virtual void StartRace()
    {

    }

    public virtual void FinishRace(bool forced)
    {
        isFinished = true;
        float extraTime = 0;
        if (forced)
        {
            extraTime = Vector3.Distance(transform.position, nextCheckpoint.transform.position);
            Checkpoint c = nextCheckpoint;
            while (c.next != null)
            {
                extraTime += c.distance;
                c = c.next;
            }
            extraTime /= movement.maxSpeed;
        }
        RaceManager.FinishRace(this, extraTime);
    }

    public virtual void RaceIsOver()
    {
        
    }

    protected virtual void FixedUpdate() {
        velocityBeforePhysicsUpdate = rb.linearVelocity;
    }

    public Transform GetHips()
    {
        return hips;
    }

    /*  updates player's movement mode and maxSpeed/locomotion accordingly */
    public virtual void SetMovementMode(Movement.Mode mode, bool initial = false)
    {
        if (initial || mode != movementMode)
        {
            prevMovementMode = movementMode;
            movementMode = mode;
            if (movement != null)
                movement.enabled = false;
            switch (mode)
            {
                // case MovementMode.Walking:
                //     break;
                case Movement.Mode.Running:
                    movement = movementOptions[(int)Movement.Mode.Running];
                    break;
                case Movement.Mode.Jetpacking:
                    if (!(movement is Jetpack))
                        movement.Land();
                    movement = movementOptions[(int)Movement.Mode.Jetpacking];
                    audioSource.clip = equipSound;
                    audioSource.Play();
                    break;
                case Movement.Mode.Gliding:
                    if (!(movement is Glider))
                        movement.Land();
                    movement = movementOptions[(int)Movement.Mode.Gliding];
                    audioSource.clip = equipSound;
                    audioSource.Play();
                    break;
                case Movement.Mode.Swimming:
                    if (!(movement is Swim))
                        movement.Land();
                    movement = movementOptions[(int)Movement.Mode.Swimming];
                    audioSource.clip = waterSound;
                    audioSource.Play();
                    break;
                case Movement.Mode.Biking:
                    if (!(movement is Bicycle))
                        movement.Land();
                    movement = movementOptions[(int)Movement.Mode.Biking];
                    audioSource.clip = bikeSound;
                    audioSource.Play();
                    break;
                case Movement.Mode.Wheeling:
                    if (!(movement is Wheeler))
                        movement.Land();
                    movement = movementOptions[(int)Movement.Mode.Wheeling];
                    break;
                case Movement.Mode.GetOffTheBoat:
                    movement = movementOptions[(int)Movement.Mode.Running];
                    break;
                
            }
            movement.enabled = true;
            animEvents.movement = movement;
            anim.SetInteger("movement_mode", (int)movementMode % 6);
        }
    }

    // If emphasizeTorso is true, then extra force will be added to the racer's hips
    // when they ragdoll, preventing them from simply retaining their animation pose
    // if hit by a laser midair
    // When newMomentum is 0,0,0, the momentum used will be simply the character's current momentum
    public virtual void Die(bool emphasizeTorso, Vector3 newMomentum = default(Vector3))
    {
        anim.enabled = false;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        ragdoll.SetRagdoll(true);
        Vector3 momentum;
        if (newMomentum == Vector3.zero)
        {
            //momentum = Vector3.ClampMagnitude(velocityBeforePhysicsUpdate, 30);
            momentum = velocityBeforePhysicsUpdate;
        }
        else
        {
            momentum = newMomentum;
        }
        ragdoll.AddMomentum(momentum, emphasizeTorso);
        dead = true;
        canRevive = false;
        try
        {
            // Deactivate jetpack particles if we're jetpacking
            Jetpack jetpack = (Jetpack)movement;
            jetpack.SetParticles(false);
        }
        catch (System.Exception)
        {
            
        }
        StartCoroutine(RevivalEnabler());
    }

    public virtual void ApplyJumpSplosion(Vector3 force)
    {
        movement.ApplyJumpSplosion(force);
        // switch (movementMode)
        // {
        //     case Movement.Mode.Running:
        //     {
        //         movement.Jump(true);
        //         movement.Launch(force);
        //     }
        //     break;
        //     case Movement.Mode.Biking:
        //     {
        //         force += 300f*Vector3.up;
        //         movement.Launch(force);
        //     }
        //     break;
        //     case Movement.Mode.Jetpacking:
        //     {
        //         //.movement.Jump(true);
        //         movement.Launch(force);
        //     }
        //     break;
        // }
    }

    protected virtual IEnumerator RevivalEnabler()
    {
        // Don't allow a revival until one second after we stop moving on the ground
        yield return new WaitUntil(() => !ragdoll.IsMoving());
        yield return new WaitForSeconds(1.5f);
        canRevive = true;
        ReviveText();
    }

    protected virtual void ReviveText()
    {
        //do nothing in base class;
    }

    public virtual void Revive(bool forceRevive = false)
    {
        if (dead && (canRevive || forceRevive))
        {
            ragdoll.SetRagdoll(false);
            anim.enabled = true;
            rb.isKinematic = false;
            GetComponent<Collider>().enabled = true;
            transform.position = hips.position;
            hips.localPosition = Vector3.zero;
            dead = false;
        }
    }

    public void ArriveAtCheckpoint(Checkpoint checkpoint)
    {
        lastCheckpoint = checkpoint;
        if (checkpoint.next != null)
            nextCheckpoint = checkpoint.next;
    }

    public virtual void EquipItem(Item item)
    {
        this.item = item;
    }

    public void SpeedBoost()
    {
        StartCoroutine(SpeedBoost(2f, 5f));
    }

    protected virtual IEnumerator SpeedBoost(float magnitude, float duration)
    {
        movement.BonusSpeed = magnitude;
        anim.speed = magnitude;
        yield return new WaitForSeconds(duration);
        movement.BonusSpeed = 1f;
        anim.speed = 1f;
    }

    public virtual void SetTarget(Transform target)
    {
        // overridden in subclass
    }

    public void DropItem()
    {
        // float back = (movement is Bicycle ? 3f : 1f);
        // Vector3 pos = transform.position - back * characterMesh.transform.forward + 0.5f * characterMesh.transform.up;
        // Instantiate(item.Child, pos, Quaternion.identity);
        // Debug.Break();
        // EquipItem(null);
    }

    public void ThrowItem(Transform target)
    {
        // float up = movement is Bicycle ? 5f : 1.5f;
        // Vector3 pos = transform.position + 2f * characterMesh.transform.forward + up * characterMesh.transform.up;
        // GameObject obj = Instantiate(item.Child, pos, Quaternion.identity);
        // Rigidbody itemRb = obj.GetComponent<Rigidbody>();
        // itemRb.linearVelocity = rb.linearVelocity;
        // itemRb.AddForce(1000 * (characterMesh.transform.forward + 0.1f * transform.up));
        // try {
        //     MelonObject melon = obj.GetComponent<MelonObject>();
        //     melon.target = target;
        //     StartCoroutine(melon.Despawn());
        // }
        // catch {
            
        // }
        // EquipItem(null);
    }

    public Vector3 GetItemSpawnPos()
    {
        float up = movement is Bicycle ? 5f : 1.5f;
        return transform.position + 2f * characterMesh.transform.forward + up * characterMesh.transform.up;
    }

    /*  plays a miscellaneus animation that is NOT defined in the animation controller */
    public void PlayMiscAnimation(AnimationClip clip)
    {
        animOverride["miscAnimation"] = clip;
        anim.runtimeAnimatorController = animOverride;
        anim.SetTrigger("misc");
    }

    public void PlayMiscSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Pick up an item
        if (other.gameObject.CompareTag("Item"))
        {
            Item item = other.GetComponent<Item>();
            item.Pickup(this);
        }
    }

    /*  check if we hit something too fast */
    protected virtual void OnCollisionEnter(Collision other)
    {
        
    }

    // Returns whether or not this racer is currently "dead"
    public bool IsDead()
    {
        return dead;
    }

    public Movement.Mode GetCurrentMovementMode()
    {
        return movementMode;
    }

    public bool isGrounded()
    {
        return movement.Grounded;
    }

    public void Land()
    {
        movement.Land();
    }
}