using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MelonObject : MonoBehaviour 
{
    protected Rigidbody rb;
    protected AudioSource audioSource;
    public Rigidbody target;
    public Rigidbody source;
    
    [SerializeField]
    protected List<AudioClip> throwSounds;
    
    [SerializeField]
    protected List<AudioClip> impactSounds;

    protected virtual void Awake() 
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }
    protected virtual void Start()
    {
        StartCoroutine(Despawn());
        if (throwSounds.Count > 0)
        {
            AudioClip clip = throwSounds[Random.Range(0, throwSounds.Count)];
            audioSource.PlayOneShot(clip);
        }
    }

    /*  if you hit somebody, they die */
    protected virtual void OnCollisionEnter(Collision other)
    {
        Entity target = other.transform.GetComponent<Entity>();
        if (target != null && other.relativeVelocity.magnitude > 7)
        {
            target.Die(true);        
            if (impactSounds.Count > 0)
            {
                AudioClip clip = impactSounds[Random.Range(0, impactSounds.Count)];
                audioSource.PlayOneShot(clip);
            }
        }
        else
        {
            MelonButton button = other.transform.GetComponent<MelonButton>();
            if (button != null)
            {
                StartCoroutine(button.Activate());
            }
        }
    }

    public IEnumerator Despawn()
    {
        yield return new WaitForSeconds(90);
        Destroy(gameObject);
    }
}