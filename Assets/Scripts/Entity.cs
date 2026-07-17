using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    public new string name;
    public Transform characterMesh;
    public Transform hips;
    protected Movement movement;
    protected Ragdoll ragdoll;
    protected Rigidbody rb;
    protected Animator anim;
    protected AnimatorOverrideController animOverride;
    protected AudioSource audioSource;
    protected Vector2 move;
    protected float moveUp, moveDown;
    protected Vector3 velocityBeforePhysicsUpdate;
    protected bool dead;
    protected bool canRevive; // when this is true, a dead racer can be revived.
    public bool invincible = false;
    protected float permanentSpeedScale = 1f;

    protected Coroutine boostCoroutine;
    protected float remainingBoostTime = 0f;
    public float dieThreshold = 40f;
    public Vector3 Forward { get => movement.Forward; }
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
        // animEvents = GetComponentInChildren<PlayerAnimationEvents>();

        //animOverride = GetComponent<AnimatorOverrideController>();
        audioSource = GetComponentInChildren<AudioSource>();
    }
    
    protected virtual void Start() 
    {
        if (ragdoll != null)
        {
            ragdoll.SetRagdoll(false);
        }
    }     

    protected virtual void Update()
    {
        if (!dead && RaceManager.IsRaceActive && !RaceManager.IsPaused)
        {
            //movement.AddMovement(move.x, moveUp - moveDown, move.y);
        }

        //Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 3f, Color.green);
    }

    protected virtual void FixedUpdate()
    {
        velocityBeforePhysicsUpdate = rb.linearVelocity;
    }

    public Transform GetHips()
    {
        return hips;
    }

    public virtual void Die(bool emphasizeTorso, Vector3 newMomentum = default(Vector3))
    {
        if (!invincible)
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
    }

    protected virtual IEnumerator RevivalEnabler()
    {
        // Don't allow a revival until one second after we stop moving on the ground
        yield return new WaitUntil(() => !ragdoll.IsMoving());
        yield return new WaitForSeconds(1.5f);
        canRevive = true;
        EnableRevive();
    }

    protected virtual void EnableRevive()
    {
        // revive ASAP
        Revive(false);
    }

    public virtual void Revive(bool forceRevive = false)
    {
        if (dead && (canRevive || forceRevive))
        {
            Vector3 landingPosition = hips.position;
            ragdoll.SetRagdoll(false);
            
            // Re-enable components
            anim.enabled = true;
            rb.isKinematic = false;
            GetComponent<Collider>().enabled = true;

            // Force the position update
            transform.position = landingPosition;
            hips.localPosition = Vector3.zero;

            // Clear velocity and sync transforms
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();

            dead = false;
        }
    }

    /*  plays a miscellaneus animation that is NOT defined in the animation controller */
    public void PlayMiscAnimation(AnimationClip clip)
    {
        animOverride["miscAnimation"] = clip;
        anim.runtimeAnimatorController = animOverride;
        anim.SetTrigger("misc");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {

    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        
    }

    // Returns whether or not this racer is currently "dead"
    public bool IsDead()
    {
        return dead;
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