using UnityEngine;

public class BreakableGlass : MonoBehaviour
{
    public GameObject unbrokenGlass;
    public Transform brokenGlassParent;
    public float power = 1f;
    public bool useExplosivePower = false;
    private bool broken = false;

    private BoxCollider boxCollider;
    private AudioSource audioSource;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();

        unbrokenGlass.SetActive(true);

        brokenGlassParent.gameObject.SetActive(false);
        
    }

    private void Break(Vector3 breakPoint)
    {
        broken = true;
        boxCollider.enabled = false;
        unbrokenGlass.SetActive(false);
        brokenGlassParent.gameObject.SetActive(true);
        if (useExplosivePower)
        {
            foreach (Transform shard in brokenGlassParent)
            {
                shard.GetComponent<Rigidbody>().AddExplosionForce(power, breakPoint, 1f);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!broken && !collision.gameObject.CompareTag("Dont KO Racer On Impact"))
        {
            Break(collision.GetContact(0).point);
            audioSource.Play();
        }
    }

    // Support OnTriggerEnter too so that we can have
    // glass-breaking events that don't slow the racers down
    void OnTriggerEnter(Collider other)
    {
        if (!broken && !other.gameObject.CompareTag("Dont KO Racer On Impact"))
        {
            // Get the point that is closest to the transform
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            Break(contactPoint);
            audioSource.Play();
        }
    }

}