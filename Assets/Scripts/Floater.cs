using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Floater : MonoBehaviour
{
    public bool bob = true;
    public float offset = 0f;

    //private bool underwater = false;
    public bool InWater { get; private set; } = false;

    private float waterHeight;
    private float prevDrag;
    private float prevAngularDrag;

    private const float waterDrag = 10f;
    private const float airDrag = 1f;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {  
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!RaceManager.IsPaused)
        {
            if (InWater)
            {
                Vector3 gravity = Physics.gravity;
                if (waterHeight > transform.position.y + offset)
                {
                    rb.linearDamping = waterDrag;
                    gravity = -0.5f * Physics.gravity;
                }
                rb.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterHeight - (transform.position.y + offset)), 0, 1));
            }
            //float y = Mathf.SmoothStep(transform.position.)
            //transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }

    public void EnterWater(float height)
    {
        Debug.Log("enter water");
        InWater = true;
        rb.useGravity = false;
        rb.angularDamping = 10f;
        waterHeight = height;

        prevDrag = rb.linearDamping;
        prevAngularDrag = rb.angularDamping;

        Racer mov = GetComponent<Racer>();
        if (mov != null)
        {
            mov.SetMovementMode(Movement.Mode.Swimming);
        }

    }

    public void ExitWater()
    {
        Debug.Log("exit water");

        InWater = false;
        rb.useGravity = true;
        rb.linearDamping = prevDrag;
        rb.angularDamping = prevAngularDrag;


        Racer mov = GetComponent<Racer>();
        if (mov != null)
        {
            mov.SetMovementMode(Movement.Mode.Running);
        }
    }
}
