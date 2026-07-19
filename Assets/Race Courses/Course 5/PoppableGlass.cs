using UnityEngine;
using System.Collections;


[System.Serializable]
[RequireComponent(typeof(Rigidbody))]
public class PoppableGlass : MonoBehaviour
{

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool popped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponentInParent<AudioSource>();

        rb.isKinematic = true;
    }


    void OnCollisionEnter(Collision collision)
    {
        // check tag to make sure it's not broken glass hitting this
        if (!popped && !collision.gameObject.CompareTag("Dont KO Racer On Impact"))
        {
            popped = true;
            audioSource.Play();
            rb.isKinematic = false;
            transform.parent = null;
            // shrink it slightly so that the frames don't hold it in
            transform.localScale *= 0.9f;
        }
    }
}
