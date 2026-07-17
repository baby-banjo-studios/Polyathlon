using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerKO : MonoBehaviour
{
    // KO any racer who touches the spinning propellers
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("collision!!!!");
        Entity target = other.gameObject.GetComponent<Entity>();
        if(target != null)
        {
            target.Die(false);
        }
    }
}
