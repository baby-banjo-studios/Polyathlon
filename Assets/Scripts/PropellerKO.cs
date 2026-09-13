using BabyBanjo.Polyathlon.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.World
{
    public class PropellerKO : MonoBehaviour
    {
        // KO any racer who touches the spinning propellers
        void OnCollisionEnter(Collision other)
        {
            Debug.Log("collision!!!!");
            Racer racer = other.gameObject.GetComponent<Racer>();
            if (racer != null)
            {
                racer.Die(false);
            }
        }
    }
}