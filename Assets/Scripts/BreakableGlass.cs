using UnityEngine;
using System.Collections;

public class BreakableGlass : MonoBehaviour
{
    public GameObject unbrokenGlass;
    public Transform brokenGlassParent;
    [Tooltip("Assign this if you want to make the broken glass a child of something else after it breaks. Useful for sliding doors.")]
    public Transform newBrokenGlassParent;
    public float power = 1f;
    public float cleanUpPercentage = 0f;
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
        if (newBrokenGlassParent != null || cleanUpPercentage > 0)
        {
            // need to create new transform array here because the contents of brokenGlassParent
            // would otherwise change while we loop through it, causing missed shards.
            Transform[] shards = new Transform[brokenGlassParent.childCount];
            for (int i = 0; i < shards.Length; i++)
            {
                shards[i] = brokenGlassParent.GetChild(i);
            }
            if (newBrokenGlassParent != null)
            {
                foreach (Transform shard in shards)
                {
                    shard.parent = newBrokenGlassParent;
                }
            }
            if (cleanUpPercentage > 0)
            {
                // Clean up a random selection of the shards, the amount determined by the percentage
                int numberToCleanUp = Mathf.CeilToInt(shards.Length * cleanUpPercentage);

                int[] indices = new int[shards.Length];
                for (int i = 0; i < indices.Length; i++)
                    indices[i] = i;

                // Fisher-Yates shuffle
                for (int i = indices.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (indices[i], indices[j]) = (indices[j], indices[i]);
                }

                for (int i = 0; i < numberToCleanUp; i++)
                {
                    StartCoroutine(CleanUpShards(shards[indices[i]]));
                }
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

    private IEnumerator CleanUpShards(Transform shard)
    {
        yield return new WaitForSeconds(Random.Range(1, 10));
        Destroy(shard.gameObject);
    }

}