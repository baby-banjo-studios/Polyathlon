using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Glider : Movement
{
    public float liftFactor = 10f;
    public float rollSpeed = 10f;
    public float pitchSpeed = 10f;
    public float dragFactor = 10f;
    private Vector3 lastVelocity;
    
    private AudioSource audioSource;
    private bool gliding = false; 
    private bool landable = false; // lets us know if we're allowed to call Land()
    private bool countingDownLandable = false; // prevents PreventFalseLanding() coroutine from being started if its running


    // Set up glider
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

        SetGlider(true);
    }

    // Put away glider
    protected override void OnDisable()
    {
        base.OnDisable();
        SetGlider(false);
    }

    public virtual void SetGlider(bool enabled)
    {
        if (enabled)
        {
            racer.BackpackMount.Equip(BackpackOptions.Glider);
        }
        else
        {
            racer.BackpackMount.Unequip(BackpackOptions.Glider);
        }
        anim.SetBool("glide", enabled);
    }

    /*  moves the player rigidbody */
    public override void AddMovement(float forward, float right)
    {
        base.AddMovement(forward, right);

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

        // if not flying
        if (grounded)
        {
            if (velocity.magnitude > 0)
            {
                if (smoothSpeed > maxSpeed)
                    smoothSpeed = smoothSpeed * Mathf.Max(Vector3.Dot(smoothSpeedDirection, velocity.normalized), 0);
                rb.linearVelocity = new Vector3(velocity.normalized.x * smoothSpeed, racer is NPC ? 0 : rb.linearVelocity.y, velocity.normalized.z * smoothSpeed);
                smoothSpeed = Mathf.Lerp(smoothSpeed, maxSpeed * bonusSpeed, Time.deltaTime);
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
            anim.SetBool("glide", true);
            anim.SetTrigger("jump");
            // don't move player, just roll and pitch
            if (forward != 0 || right != 0)
            {
                characterMesh.RotateAround(racer.hips.position, racer.hips.right, pitchSpeed * right * Time.deltaTime);
                characterMesh.RotateAround(racer.hips.position, racer.hips.up, rollSpeed * - forward * Time.deltaTime);
            }
            RaycastHit hit;
            
            bool castResult = Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.2f, 0), out hit);
            // allow exception to NPC who were previously wheeling because sometimes they zoom through the gliders so fast that they somehow never become landable
            if ((landable || (racer is NPC && racer.prevMovementMode == Movement.Mode.Wheeling)) && castResult)
            {
                characterMesh.position = transform.position;
                characterMesh.localEulerAngles = new Vector3(0, characterMesh.localEulerAngles.y, 0);
                rb.linearDamping = 0;
                Land();
            }
        }
        // blend speed in animator to match pace of footsteps
        // normal movement (character moves independent of camera)
        
        speed = Mathf.SmoothStep(speed, actualVelocity.magnitude, Time.deltaTime * 20);
    
        anim.SetFloat("speed", speed, dampTime, Time.deltaTime);
        anim.SetBool("grounded", grounded);
        
    }
    
    /*  causes the player to toggle their glider */
    public override void Jump(bool hold)
    {
        base.Jump(hold);
        if (grounded)
        {
            if (racer is NPC || Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.2f, 0)))
            {
                BeginGliding();
                StartCoroutine(JumpAtNextFrame());
                StartCoroutine(PreventFalseLanding(racer is NPC ? 0.5f : 0.2f));
            }
        }
    }

    private void BeginGliding()
    {
        // anim.ResetTrigger("land");
        lastVelocity = new Vector3(0,0,0);
        grounded = false;
        gliding = true;
        rb.linearDamping = 0.01f;
        anim.SetTrigger("jump");
    }

    // Need to wait for next frame here because otherwise the NPC may attempt to jump
    // before the navmesh agent is disabled, which will not work.
    private IEnumerator JumpAtNextFrame()
    {
        yield return null;
        rb.AddForce(Vector3.up * jumpForce + characterMesh.transform.forward * jumpForce);
    }

    // Handle the gliding in the physics loop
    void FixedUpdate()
    {
        if (gliding)
        {
            // lift = local forward speed * cos(angle off of the xy plane) * lift factor * cos(roll angle)
            Vector3 lift = Vector3.Project(rb.linearVelocity, characterMesh.forward) * Mathf.Cos(Vector3.Angle(characterMesh.forward, Vector3.forward)) * liftFactor * Mathf.Cos(Vector3.Angle(characterMesh.right, Vector3.right));
            Vector3 verticalDragAmount = - Vector3.Project(rb.linearVelocity, characterMesh.up) * dragFactor;
            if (verticalDragAmount.y < 0)
            {
                verticalDragAmount *= 2;
            }

            // apply lift and vertical drag
            rb.AddForce(Time.fixedDeltaTime * lift.magnitude * characterMesh.up + verticalDragAmount * Time.fixedDeltaTime);
            
        }
        else if (!(racer is NPC) && !Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.5f, 0)))
        {
            BeginGliding();
            landable = true;
        }
    }

    // Without this, when the racer first takes off, the raycast in AddMovement that
    // determines if they're on the ground returns true, causing the player to do the
    // landing animation while in the air.
    // This coroutine prevents that by setting a bool (landable) after a short time of being airborn
    // that allows the racer to land
    private IEnumerator PreventFalseLanding(float waitTime)
    {
        yield return new WaitForEndOfFrame();

        // prevent other countdowns from being started
        if (countingDownLandable)
        {
            yield break;
        }
        countingDownLandable = true;
        yield return new WaitForSeconds(waitTime);
        landable = true;
        countingDownLandable = false;
    }

    /*  grounds the player after a jump is complete */
    public override void Land()
    {
        base.Land();
        
        landable = false;
        gliding = false;
        rb.linearDamping = 0;
        // anim.SetTrigger("land");
        smoothSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        smoothSpeedDirection = new Vector3(rb.linearVelocity.normalized.x, 0, rb.linearVelocity.normalized.z).normalized;
        if (racer is NPC)
        {
            ((NPC)racer).Land();
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
            rb.isKinematic = true;
            rb.isKinematic = false;
            ((NPC)racer).GoToNearestJetpack();
            Debug.Log(gameObject.name + " NPC must go to nearest jetpack");
            StartCoroutine(LostNPCContingency());
        }
        else
        {
            cameraController.ResetXMinMax();
        }
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        Launch(force);
        Jump(true);
    }

    // Some NPCs are truly lost. This should help them.
    private IEnumerator LostNPCContingency()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            if (rb.linearVelocity.magnitude <= 0.01f)
            {
                ((NPC)racer).GoToNearestJetpack();
            } else if (rb.linearVelocity.magnitude > 50f)
            {
                rb.isKinematic = true;
                yield return null;
                rb.isKinematic = false;
            }
        }
    }
    
    protected override void LateUpdate()
    {
        // Prevent short glider flights from happening twice due to a lingering trigger
        anim.ResetTrigger("jump");
    }
}
