using UnityEngine;

public class BananaObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) 
    {
        Racer racer = other.gameObject.GetComponentInParent<Racer>();
        if (racer != null)
        {
            racer.Die(false);
            Destroy(this.gameObject, 2f);
        }    
    }   
}