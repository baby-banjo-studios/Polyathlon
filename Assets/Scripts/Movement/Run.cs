using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Run : Movement
{
    public float Direction { get => actualVelocity == Vector3.zero ? 0f : Mathf.Abs(Quaternion.LookRotation(actualVelocity, Vector3.up).eulerAngles.y - characterMesh.transform.rotation.eulerAngles.y); }

    private bool preventingJumpLock = false;
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
    }

    /*  moves the player rigidbody */
    public override void AddMovement(float forward, float up, float right)
    {
        base.AddMovement(forward, up, right);

        if (!launched)
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

            // moved from update
            if (velocity.magnitude > 0)
            {
                rb.linearVelocity = new Vector3(velocity.normalized.x * smoothSpeed, rb.linearVelocity.y, velocity.normalized.z * smoothSpeed);
                smoothSpeed = Mathf.Lerp(smoothSpeed, maxSpeed * boostSpeedScale * PermanentSpeedScale * PhysicalSpeedScale, Time.deltaTime);
                // rotate the character mesh if enabled
                
                characterMesh.rotation = Quaternion.Lerp(characterMesh.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationSpeed);
                
            }
            else
            {
                smoothSpeed = Mathf.Lerp(smoothSpeed, 0, Time.deltaTime*8);
            }
        }
    
        // if the player landed, enable another jump
        if (!grounded)
        {
            RaycastHit hit;
            float vel = rb.linearVelocity.y;
            if ((falling || vel < -0.1f) && Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.2f, 0), out hit))
            {
                falling = false;
                Land();
            }
            else if (Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.2f, 0), out hit))
            {
                StartCoroutine(PreventJumpLock());
            }
        }
        // blend speed in animator to match pace of footsteps
        // normal movement (character moves independent of camera)
        
        speed = Mathf.SmoothStep(speed, actualVelocity.magnitude, Time.deltaTime * 20);
    
        anim.SetFloat("speed", speed / PhysicalSpeedScale, dampTime, Time.deltaTime);
        anim.SetBool("grounded", grounded);
        //Debug.Log("velocity" + velocity);
    }

    // this is a failsafe in case the player presses jump at the instant
    // that somehow causes them to land without land being called.
    // Basically if we haven't landed after 4 seconds, we're landing
    private IEnumerator PreventJumpLock()
    {
        if (preventingJumpLock)
            yield break;
        else
        {
            preventingJumpLock = true;
            float maxJumpFixTime = 4;
            float jumpFixTimer = 0;
            while (jumpFixTimer < maxJumpFixTime && !grounded)
            {
                jumpFixTimer += Time.deltaTime;
                yield return null;
            }
            if (jumpFixTimer >= maxJumpFixTime && rb.linearVelocity.y > -10)
            {
                Debug.Log("Fixing jump!");
                falling = false;
                grounded = true;
            }
            preventingJumpLock = false;
        }
    }
    
    /* causes the player to jump */
    public override void Jump(bool hold)
    {
        base.Jump(hold);
        if (grounded && hold)
        {
            if (Physics.Linecast(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -0.1f, 0)))
            {
                // anim.ResetTrigger("land");       

                rb.AddForce(Vector3.up * jumpForce);
                grounded = false;
                falling = false;
                anim.SetTrigger("jump");
            }
        }
    }

    /*  grounds the player after a jump is complete */
    public override void Land()
    {
        base.Land();

        Debug.Log(gameObject.name + " has landed!!!");
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        Jump(true);
        Launch(force);
    }
}
