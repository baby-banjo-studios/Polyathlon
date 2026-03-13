using UnityEngine;

public class BoostRamp : MonoBehaviour
{

    void OnTriggerEnter (Collider other)
    {
        Racer racer = other.gameObject.GetComponent<Racer>();
        if (racer != null)
        {
            racer.SpeedBoost(4, 1.5f);
        }
    }
}
