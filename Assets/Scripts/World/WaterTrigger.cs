using BabyBanjo.Polyathlon.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.World
{
    [RequireComponent(typeof(BoxCollider))]
    public class WaterTrigger : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            Floater f = other.GetComponent<Floater>();
            if (f != null)
            {
                f.EnterWater(transform.position.y);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Floater f = other.GetComponent<Floater>();
            if (f != null)
            {
                f.ExitWater();
            }
        }
    }
}