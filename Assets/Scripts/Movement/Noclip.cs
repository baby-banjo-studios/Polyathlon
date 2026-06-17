using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Noclip : Movement
{
    public float Direction { get => actualVelocity == Vector3.zero ? 0f : Mathf.Abs(Quaternion.LookRotation(actualVelocity, Vector3.up).eulerAngles.y - characterMesh.transform.rotation.eulerAngles.y); }

    private bool preventingJumpLock = false;
    protected override void OnEnable() 
    {
        base.OnEnable();
        rb.mass = 1;
        rb.linearDamping = 0.8f;
        rb.angularDamping = 0.7f;
        //rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        mainCollider.enabled = false;

        maxSpeed = runSpeed;
        acceleration = runAcceleration;
        angularSpeed = 120f;
        smoothSpeed = rb.linearVelocity.magnitude;

        Land();
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
        translation += up * Vector3.up;
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
            rb.linearVelocity = new Vector3(velocity.normalized.x * smoothSpeed, velocity.normalized.y * smoothSpeed, velocity.normalized.z * smoothSpeed);
            smoothSpeed = Mathf.Lerp(smoothSpeed, maxSpeed * bonusSpeed, Time.deltaTime);
            
            // rotate the character mesh if enabled
            Vector3 velocity2D = new Vector3(velocity.x, 0f, velocity.z);
            if (velocity2D.magnitude > 0)
            {
                characterMesh.rotation = Quaternion.Lerp(characterMesh.rotation, Quaternion.LookRotation(velocity2D), Time.deltaTime * rotationSpeed);
            }            
        }
        else
        {
            smoothSpeed = Mathf.Lerp(smoothSpeed, 0, Time.deltaTime*8);
        }
            
        Vector3 actualVelocity2D = new Vector3(actualVelocity.x, 0f, actualVelocity.z);
        speed = Mathf.SmoothStep(speed, actualVelocity2D.magnitude, Time.deltaTime * 20);
    
        anim.SetFloat("speed", speed, dampTime, Time.deltaTime);
    }
    
    public override void Jump(bool hold)
    {
        
    }
    public override void Land()
    {        
        anim.SetBool("grounded", grounded);
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        
    }
}
