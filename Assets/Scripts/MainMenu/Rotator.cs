using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 rotation_deg_per_sec;

    private void Update()
    {
        transform.Rotate(rotation_deg_per_sec * Time.deltaTime);
    }
}