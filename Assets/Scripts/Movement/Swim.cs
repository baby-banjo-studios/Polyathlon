using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Swim : Movement
{    
    public float Direction { get => actualVelocity == Vector3.zero ? 0f : Mathf.Abs(Quaternion.LookRotation(actualVelocity, Vector3.up).eulerAngles.y - characterMesh.transform.rotation.eulerAngles.y); }

    protected override void OnEnable() 
    {
        base.OnEnable();
        rb.mass = 1;
        rb.angularDamping = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxSpeed = runSpeed;
        acceleration = swimAcceleration;
        angularSpeed = 120f;
        // prevent the player from moving the camera underwater
        if (cameraController != null)
        {
            cameraController.SetXMinMax(0f, 60f);
        }
    }

    /*  moves the player rigidbody */
    public override void AddMovement(float forward, float up, float right)
    {
        base.AddMovement(forward, up, right);
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
            smoothSpeed = Mathf.Lerp(smoothSpeed, maxSpeed * boostSpeedScale * permanentSpeedScale, Time.deltaTime);
            // rotate the character mesh if enabled
            characterMesh.rotation = Quaternion.Lerp(characterMesh.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationSpeed);
        }
        else
        {
            smoothSpeed = Mathf.Lerp(smoothSpeed, 0, Time.deltaTime*8);
        }
        
        // blend speed in animator to match pace of footsteps
        // normal movement (character moves independent of camera)
        speed = Mathf.SmoothStep(speed, actualVelocity.magnitude, Time.deltaTime * 20);
        anim.SetFloat("speed", speed, dampTime, Time.deltaTime);
        anim.SetBool("grounded", grounded);
    }

    
    /*  causes the player to jump */
    public override void Jump(bool hold)
    {
        base.Jump(hold);
    }

    /*  grounds the player after a jump is complete */
    public override void Land()
    {
        base.Land();
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        // i have no idea what would happen if you do this when swimming
        // add some lift maybe? 
        force += Vector3.up * 300f;
        Launch(force);
    }
}
