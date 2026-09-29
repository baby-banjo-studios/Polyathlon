using UnityEngine;

public class HorseItem : Item
{
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
    }

    public override void Pickup(Racer racer)
    {
        racer.SetMovementMode(Movement.Mode.Horseback);
        base.Pickup(racer);
    }
}
