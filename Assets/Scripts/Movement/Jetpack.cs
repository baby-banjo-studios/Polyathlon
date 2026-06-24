using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Jetpack : Movement
{
    public AudioClip thrustSound;

    protected ParticleSystem[] jetpackExhaust;
    protected bool fireJetpack; // this tells us whether we will be firing the jetpack during this frame
    
    public float Direction { get => actualVelocity == Vector3.zero ? 0f : Mathf.Abs(Quaternion.LookRotation(actualVelocity, Vector3.up).eulerAngles.y - characterMesh.transform.rotation.eulerAngles.y); }
    
    private AudioSource audioSource;
    private bool landable = false; // lets us know if we're allowed to call Land()
    private bool countingDownLandable = false; // prevents PreventFalseLanding() coroutine from being started if its running

    [SerializeField] private float launchDuration = 0.8f;
    [SerializeField] private float launchCooldown = 0.8f;
    private float launchElapsedTime = 0f;

    private void Start()
    {
        jetpackExhaust = racer.BackpackMount.jetpack.transform.GetComponentsInChildren<ParticleSystem>();    
        audioSource = GetComponent<AudioSource>();
    }

    // Set up jetpack
    protected override void OnEnable() 
    {
        base.OnEnable();
        rb.mass = 1;
        rb.angularDamping = 0;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxSpeed = runSpeed;
        acceleration = runAcceleration;
        angularSpeed = 120f;
        smoothSpeed = rb.linearVelocity.magnitude;

        SetJetpack(true);
    }

    // Put away jetpack
    protected override void OnDisable()
    {
        base.OnDisable();
        SetJetpack(false);
    }

    public virtual void SetJetpack(bool enabled)
    {
        if (enabled)
        {
            racer.BackpackMount.Equip(BackpackOptions.Jetpack);
        }
        else
        {
            racer.BackpackMount.Unequip(BackpackOptions.Jetpack);
        }
        fireJetpack = false;
        anim.SetBool("jetpack", enabled);
    }

    /*  moves the player rigidbody */
    public override void AddMovement(float forward, float up, float right)
    {
        base.AddMovement(forward, up, right);

        // keep track of how long we have been launched for
        // if we pass the predefined threshold, un-launch
        if (launched)
        {
            launchElapsedTime += Time.deltaTime;
            if (launchElapsedTime >= launchDuration + launchCooldown)
            {
                launched = false;
                Jump(false);
            }
        }

        // if we're actually launched, turn jetpack on
        if (launched)
        {
            Jump(true);
        }
        // otherwise, normal movement
        else
        {
            Vector3 translation = Vector3.zero;
            // for npcs
            if (cameraController == null)
            {
                translation += right * transform.forward;
                translation += forward * transform.right;    
            }
            // for players
            else
            {
                translation += right * cameraController.transform.forward;
                translation += forward * cameraController.transform.right;
            }
            
            translation.y = 0;
            if (translation.magnitude > 0)
            {
                velocity = translation;
            }
            else
            {
                velocity = Vector3.zero;
            }
        }

        // if not flying
        if (grounded)
        {
            if (velocity.magnitude > 0)
            {
                if (smoothSpeed > maxSpeed)
                    smoothSpeed = smoothSpeed * Mathf.Max(Vector3.Dot(smoothSpeedDirection, velocity.normalized), 0);
                rb.linearVelocity = new Vector3(velocity.normalized.x * smoothSpeed, rb.linearVelocity.y, velocity.normalized.z * smoothSpeed);
                smoothSpeed = Mathf.Lerp(smoothSpeed, maxSpeed * boostSpeedScale * permanentSpeedScale, Time.deltaTime);
                // rotate the character mesh if enabled
                
                characterMesh.rotation = Quaternion.Lerp(characterMesh.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationSpeed);
                
            }
            else
            {
                smoothSpeed = Mathf.Lerp(smoothSpeed, 0, Time.deltaTime*8);
            }
        }
        // if they are flying
        else if (!grounded)
        {
            // don't move player, just rotate
            if (forward != 0 || right != 0)
                characterMesh.rotation = Quaternion.Lerp(characterMesh.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationSpeed);
            RaycastHit hit;
            // allow exception to NPC from landable rule because their navmesh gets messed up otherwise
            if ((landable || racer is NPC) && Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.2f, 0), out hit))
            {
                Land();
                Debug.Log(gameObject.name + "has landed");
            }
        }
        // blend speed in animator to match pace of footsteps
        // normal movement (character moves independent of camera)
        
        speed = Mathf.SmoothStep(speed, actualVelocity.magnitude, Time.deltaTime * 20);
    
        anim.SetFloat("speed", speed, dampTime, Time.deltaTime);
        anim.SetBool("grounded", grounded);
        
    }
    
    /*  causes the player to fire their jetpack */
    public override void Jump(bool hold)
    {
        base.Jump(hold);
        if (!hold)
        {
            // shut down the jetpack
            fireJetpack = false;
        }
        else
        {
            // Start firing the jetpack
            StartCoroutine(JetpackThrust());    
        }
        if (grounded && hold)
        {
            grounded = false;
        }
        if (cameraController != null)
        {
            cameraController.SetXMinMax(-60f, 70f);
        }
    }

    // Handle the jetpack thrusting
    private IEnumerator JetpackThrust()
    {
        // no need to restart the coroutine if we've already started it
        if (fireJetpack)
        {
            yield break;
        }
        if (!landable)
            StartCoroutine(PreventFalseLanding());
        fireJetpack = true;
        SetParticles(true);
        // Handle thrust
        audioSource.clip = thrustSound;
        audioSource.loop = true;
        audioSource.Play();
        // INTERRUPTABLE: if fireJetpack is false, exit
        while(fireJetpack && !racer.IsDead())
        {
            rb.AddForce(racer.BackpackMount.jetpack.transform.transform.up * jetpackForce * Time.deltaTime * boostSpeedScale * permanentSpeedScale);
            Vector3 clampedVelocity = Vector3.ClampMagnitude(rb.linearVelocity, jetpackSpeed * boostSpeedScale * permanentSpeedScale);
            if (launched)
            {
                if (launchElapsedTime > launchDuration)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, clampedVelocity, (launchElapsedTime - launchDuration) / launchCooldown);
                }
            }
            else
            {
                rb.linearVelocity = clampedVelocity;
            }
            grounded = false;
            anim.SetTrigger("jump");
            yield return null;
        }
        SetParticles(false);
        audioSource.loop = false;
        audioSource.Stop();
    }

    // Without this, when the racer first takes off, the raycast in AddMovement that
    // determines if they're on the ground returns true, causing the player to do the
    // landing animation while in the air.
    // This coroutine prevents that by setting a bool (landable) after a short time of being airborn
    // that allows the racer to land
    private IEnumerator PreventFalseLanding()
    {
        // prevent other countdowns from being started
        if (countingDownLandable)
        {
            yield break;
        }
        countingDownLandable = true;
        yield return new WaitForSeconds(0.2f);
        landable = true;
        countingDownLandable = false;
    }

    public override void Launch(Vector3 force)
    {
        base.Launch(force);
    }

    public void SetParticles(bool fire)
    {
        // Handle particle systems for the exhaust
        if (fire)
        {
            foreach(ParticleSystem nozzle in jetpackExhaust)
            {
                nozzle.Play();
            }
        }
        else
        {
            fireJetpack = false; // this is only redundant sometimes
            foreach(ParticleSystem nozzle in jetpackExhaust)
            {
                nozzle.Stop();
            }
        }
    }

    /*  grounds the player after a jump is complete */
    public override void Land()
    {
        base.Land();
        
        landable = false;
        // anim.SetTrigger("land");
        smoothSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        smoothSpeedDirection = new Vector3(rb.linearVelocity.normalized.x, 0, rb.linearVelocity.normalized.z).normalized;
        if (racer is NPC)
        {
            ((NPC)racer).Land();
        }
        else
        {
            cameraController.ResetXMinMax();
        }
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        Launch(force);
        // jetpack doesn't force the racer to land before regaining control
        launchElapsedTime = 0f;
    }

    protected override void LateUpdate()
    {
        // Prevent short jetpack flights from happening twice due to a lingering trigger
        anim.ResetTrigger("jump");
    }
}
