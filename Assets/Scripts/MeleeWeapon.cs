using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    private Entity owner;
    private bool armed;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        owner = null;
        armed = false;
    }

    public void Initialize(Entity owner)
    {
        this.owner = owner;
    }

    public void Arm(bool value)
    {
        armed = value;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (armed)
        {
            if (other.gameObject.TryGetComponent(out Entity entity))
            {
                if (owner != entity)
                {
                    entity.Die(false);
                }
            }
        }
    }

}