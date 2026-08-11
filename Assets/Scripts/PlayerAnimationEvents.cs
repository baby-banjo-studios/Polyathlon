using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAnimationEvents : MonoBehaviour
{
    public Movement movement;
    private Entity parent;
    private AudioSource aud;
    [Header("Footstep sounds")]
    public AudioClip[] concreteSteps;
    public AudioClip[] plasticSteps;

    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
        parent = GetComponentInParent<Entity>();
    }

    /*  plays a footstep sound when triggered by an animation */
    public void Footstep(float targetWalkSpeed)
    {
        // if the state of the target speed is not matched, do not play a footstep.
        // this prevents the blended animation of 2 motion states playing the sound effect twice.
        bool doStep = GetMovementState(targetWalkSpeed) == GetMovementState(movement.Velocity.magnitude);
        if (doStep)
        {
            if (targetWalkSpeed == 2.0f)
            {
                aud.volume = 0.05f;
            }
            else
            {
                aud.volume = 0.25f;
            }
            aud.PlayOneShot(ShuffleAudioArray(plasticSteps));
        }
    }

    /*  called when the player reaches the peak of their jump to enable them to land
        prevents triggering landing at the start of the jump */
    public void StartFalling()
    {
        movement.Falling = true;
    }

    /*  called when a NPC Wizard uses a fireball attack */ 
    public void CastFireball()
    {
        if (parent is HostileNPC hnpc)
        {
            hnpc.SpawnProjectile();
        }
    }

    /*  called when a NPC begins a melee attack */ 
    public void AttackStart()
    {
        if (parent is HostileNPC hnpc)
        {
            hnpc.SetAttacking(true);
        }
    }

    /*  called when a NPC ends a melee attack */ 
    public void AttackEnd()
    {
        if (parent is HostileNPC hnpc)
        {
            hnpc.SetAttacking(false);
        }
    }

    /*  returns the movement state category of a float passed in through an animation */
    private int GetMovementState(float speed)
    {
        if (speed < 0.5f)
            return 0; //idle
        if (speed < 3)
            return 1; //walking
        if (speed < 6)
            return 2; //running
        return 3; //sprinting
    }

    /*  ensures that the same footstep sound will not be played twice in a row */
    private AudioClip ShuffleAudioArray(AudioClip[] arr)
    {
        AudioClip temp = arr[0];
        int rand = Random.Range(1, arr.Length);
        arr[0] = arr[rand];
        arr[rand] = temp;
        return arr[0];
    }
}
