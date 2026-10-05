using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HorseItem : Item
{
    public List<Material> bodyMaterials;
    public List<Material> hairMaterials;

    public SkinnedMeshRenderer bodyMesh;
    public SkinnedMeshRenderer hairMesh;
    [SerializeField]
    private bool materialsAssigned = false;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        Animator horseAnim = GetComponentInChildren<Animator>();
        if (horseAnim != null)
        {
            horseAnim.SetBool("idle", true);
        }
        if (!materialsAssigned)
        {
            AssignMaterials();
        }
    }

    public override void Pickup(Racer racer)
    {
        if (!pickupDisabled)
        {
            racer.InitializeHorse(bodyMesh.material, hairMesh.material, transform.forward);
            racer.SetMovementMode(Movement.Mode.Horseback);
            base.Pickup(racer);

            if (dontRespawn)
            {
                Destroy(this.gameObject);
            }
        }
    }

    protected override IEnumerator RespawnItem()
    {
        AssignMaterials();
        yield return base.RespawnItem();
    }

    private void AssignMaterials()
    {
        int materialIdx = Random.Range(0, bodyMaterials.Count);
        bodyMesh.material = bodyMaterials[materialIdx];
        hairMesh.material = hairMaterials[materialIdx];
        materialsAssigned = true;
    }

    public void AssignMaterials(Material hairMat, Material bodyMat)
    {
        bodyMesh.material = hairMat;
        hairMesh.material = bodyMat;
        materialsAssigned = true;
    }
}
