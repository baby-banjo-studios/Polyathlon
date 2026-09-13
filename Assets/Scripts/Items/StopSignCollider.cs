using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class StopSignCollider : MonoBehaviour
    {
        private StopSignObject parent;
        private void Awake()
        {
            parent = GetComponentInParent<StopSignObject>();
            if (parent == null)
            {
                Debug.LogError("StopSignCollider failed to locate parent StopSignObject");
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            parent.CollisionDetected(other);
        }
    }
}