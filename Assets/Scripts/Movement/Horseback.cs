using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Horseback : Movement
{
    [SerializeField]
    private GameObject horse;
    [SerializeField]
    private GameObject horsePrefab;
    [SerializeField]
    private SkinnedMeshRenderer bodyMesh, hairMesh;
    private Animator horseAnim;
    [SerializeField]
    private Transform charMountPos;
    [SerializeField]
    private Transform dismountPos;

    public float Direction { get => actualVelocity == Vector3.zero ? 0f : Mathf.Abs(Quaternion.LookRotation(actualVelocity, Vector3.up).eulerAngles.y - characterMesh.transform.rotation.eulerAngles.y); }

    private bool preventingJumpLock = false;
    private float rotationValue = 0f;
    [SerializeField]
    private float rotSpeed = 45;
    [SerializeField]
    private float cooldownTimeAfterDismount = 1f;

    // IK
    [SerializeField]
    private TwoBoneIKConstraint leftHandIK, rightHandIK;

    public override Vector3 Forward { get => horse.transform.forward; }

    protected override void Awake()
    {
        base.Awake();
        if (horse != null)
        {
            horseAnim = horse.GetComponentInChildren<Animator>(true);
        } 
    }

    protected override void OnEnable() 
    {
        base.OnEnable();
        horse.SetActive(true);

        rb.mass = 1;
        rb.angularDamping = 0;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxSpeed = horseSpeed;
        acceleration = runAcceleration;
        angularSpeed = 120f;
        smoothSpeed = rb.linearVelocity.magnitude;

        characterMesh.transform.parent = charMountPos;
        characterMesh.transform.localPosition = Vector3.zero;
        characterMesh.transform.localEulerAngles = Vector3.zero;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        horse.SetActive(false);

        characterMesh.transform.parent = this.transform;
        characterMesh.SetSiblingIndex(0);
        characterMesh.transform.localPosition = Vector3.zero;
        characterMesh.transform.localEulerAngles = Vector3.zero;
    }

    /*  moves the player rigidbody */
    public override void AddMovement(float forward, float up, float right)
    {
        base.AddMovement(forward, up, right);
        if (!launched)
        {
            Vector3 translation = Vector3.zero;
            float rot = 0;
            
            if (right > 0)
            {
                translation += right * transform.forward;
            }
            rot += forward * rotSpeed;
            
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
                
                horse.transform.rotation = Quaternion.Lerp(horse.transform.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationSpeed);
                
            }
            else
            {
                smoothSpeed = Mathf.Lerp(smoothSpeed, 0, Time.deltaTime*8);
            }

            if (rb.linearVelocity.magnitude > 0.001 && forward != 0)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + (rot * Time.deltaTime), 0);
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
    
        //anim.SetFloat("speed", speed / PhysicalSpeedScale, dampTime, Time.deltaTime);
        anim.SetBool("grounded", grounded);
        horseAnim.SetBool("grounded", grounded);
        float animSpeed = speed / PhysicalSpeedScale;
        horseAnim.SetFloat("speed", animSpeed, dampTime, Time.deltaTime);
        //Debug.Log("velocity" + velocity);

        // quick fix to ensure player doesnt rock back like an idiot lol
        // the horse bone that gives the best movement also makes the player pitch up
        characterMesh.transform.rotation = horse.transform.rotation;
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
                horseAnim.SetTrigger("jump");
            }
        }
    }

    /*  grounds the player after a jump is complete */
    public override void Land()
    {
        base.Land();

        Debug.Log(gameObject.name + " has landed!!!");
    }

    public override void Dismount()
    {
        GameObject spawnedHorse = Instantiate(horsePrefab, horse.transform.position, horse.transform.rotation);
        if (spawnedHorse.TryGetComponent(out HorseItem horseItem))
        {
            horseItem.dontRespawn = true;
            horseItem.AssignMaterials(bodyMesh.material, hairMesh.material);
            horseItem.Cooldown(cooldownTimeAfterDismount);
        }
        racer.SetMovementMode(Mode.Running);
        //racer.transform.position = dismountPos.position;
        racer.WarpTo(dismountPos.position, true);
    }

    public override void StartSpeedBoost(float magnitude)
    {
        base.StartSpeedBoost(magnitude);
        horseAnim.speed = BoostSpeedScale * PermanentSpeedScale;
    }
    
    public override void EndSpeedBoost()
    {
        base.EndSpeedBoost();
        horseAnim.speed = PermanentSpeedScale;
    }

    public override void ApplyJumpSplosion(Vector3 force)
    {
        Jump(true);
        Launch(force);
    }

    public void InitializeHorse(Material bodyMat, Material hairMat, Vector3 horseForward)
    {
        horse.transform.forward = horseForward;
        bodyMesh.material = bodyMat;
        hairMesh.material = hairMat;
    }
}
