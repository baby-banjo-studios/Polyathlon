using UnityEngine;


// Holds an object in place until the first collision
[System.Serializable]
[RequireComponent(typeof(Rigidbody))]
public class FreezeUntilTouched : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void OnCollisionEnter(Collision other)
    {
        rb.isKinematic = false;
    }
}
