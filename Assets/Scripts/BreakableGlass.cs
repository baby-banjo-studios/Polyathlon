using UnityEngine;

public class BreakableGlass : MonoBehaviour
{
    public GameObject unbrokenGlass;
    public Transform brokenGlassParent;
    public float power = 1f;
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
        foreach (Transform shard in brokenGlassParent)
        {
            shard.GetComponent<Rigidbody>().AddExplosionForce(power, breakPoint, 1f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!broken)
        {
            Break(collision.GetContact(0).point);
            audioSource.Play();
        }
    }

}