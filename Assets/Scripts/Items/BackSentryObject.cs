using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackSentryObject : MonoBehaviour
{
    [SerializeField] private LaserCannon cannon;
    [SerializeField] private int numberOfShots;
    [SerializeField] private float waitTimeWhenFinished = 0.5f;
    [SerializeField]
    protected List<AudioClip> equipSounds;
    protected AudioSource audioSource;
    private BackpackMount ownerBackpackMount;

    protected virtual void Awake() 
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(Racer owner)
    {
        gameObject.SetActive(true);
        cannon.owner = owner;
        ownerBackpackMount = owner.BackpackMount;

        if (equipSounds.Count > 0)
        {
            AudioClip clip = equipSounds[Random.Range(0, equipSounds.Count)];
            audioSource.PlayOneShot(clip);
        }

        StartCoroutine(CannonCoroutine());
    }

    private IEnumerator CannonCoroutine()
    {
        ownerBackpackMount.Equip(BackpackOptions.BackSentry);

        if (cannon.owner.movementMode == Movement.Mode.Wheeling)
        {   // Too jittery while wheeling so no time to slerp
            yield return StartCoroutine(cannon.AimAndShootCoroutine(numberOfShots, false));
        }
        else
        {
            yield return StartCoroutine(cannon.AimAndShootCoroutine(numberOfShots));
        }
        yield return new WaitForSeconds(waitTimeWhenFinished);

        ownerBackpackMount.Unequip(BackpackOptions.BackSentry);
    }
}