using UnityEngine;

public class BananaObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) 
    {
        Entity target = other.gameObject.GetComponentInParent<Entity>();
        if (target != null)
        {
            target.Die(false);
            Destroy(this.gameObject, 2f);
        }    
    }   
}