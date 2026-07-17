using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StopSignObject : MonoBehaviour
{
    protected const string defaultText = "STOP";
    [SerializeField]
    protected float textChangeDuration = 2f;
    [SerializeField]
    protected float maxRotationDeg = 60f;
    [SerializeField]
    protected float maxRotationRate = 10f;
    [SerializeField]
    protected float maxStretchRate = 10f;
    [SerializeField]
    protected float maxXZDistToTargetGrounded = 10f;
    protected float maxXZDistToTargetDynamic = 25f;
    [SerializeField]
    protected float killForce = 300f;

    [SerializeField]
    protected Transform stopSignCenter, post, pole, db;
    [SerializeField]
    protected TextMeshPro stopSignText;
    [SerializeField]
    protected AudioClip[] hitSounds;
    [SerializeField]
    protected string[] hitTexts;
    protected AudioSource audioSource;
    [SerializeField]
    protected bool dynamicHeight = false;
    protected float offsetFromGround;
    protected Vector3 forwardVector;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("StopSignObject is missing AudioSource");
        }
        offsetFromGround = stopSignCenter.localPosition.y;
    }

    private void Start()
    {
        stopSignText.text = defaultText;
    }

    public void Initialize(Racer owner, bool dynamicHeight)
    {
        this.dynamicHeight = dynamicHeight;
        forwardVector = owner.Forward;
    }

    private void Update()
    {            
        float minDist = Mathf.Infinity;
        float maxXZDistToTarget = dynamicHeight ? maxXZDistToTargetDynamic : maxXZDistToTargetGrounded;
        Racer target = null;
        Vector3 targetRelativeOffset = Vector3.zero;

        foreach (Racer racer in RaceManager.GetListOfRacers())
        {
            Vector3 relativeOffset = transform.InverseTransformPoint(racer.transform.position);
            relativeOffset.y = 0;
            if (relativeOffset.z < 0)
            {            
                Vector3 distanceToTarget = racer.hips.transform.position - transform.position;
                distanceToTarget.y = 0;
                float sqDist = distanceToTarget.sqrMagnitude;
                if (sqDist < minDist && sqDist <= maxXZDistToTarget * maxXZDistToTarget)
                {
                    minDist = sqDist;
                    target = racer;
                    targetRelativeOffset = relativeOffset;
                }
            }
        } 
        Vector3 desiredUpVector = Vector3.up;
        Vector3 desiredLocalScale = new Vector3(1f, offsetFromGround, 1f);
        Vector3 desiredStopSignPos = new Vector3(0f, offsetFromGround, 0f);
        if (target != null)
        {
            float maxRot = targetRelativeOffset.x > 0 ? -maxRotationDeg : maxRotationDeg;
            Vector3 worldDirectionToTarget = (target.hips.transform.position - transform.position);
            Vector3 localDirectionToTarget = transform.InverseTransformDirection(worldDirectionToTarget);
            localDirectionToTarget.z = 0;
            desiredUpVector = transform.TransformDirection(localDirectionToTarget.normalized);

            if (dynamicHeight)
            {
                Vector3 xyDirToTarget = transform.TransformDirection(localDirectionToTarget);
                float requiredDistance = xyDirToTarget.magnitude;
                pole.localScale = new Vector3(1f, requiredDistance, 1f);
                stopSignCenter.localPosition = new Vector3(0f, requiredDistance, 0f);
            }
        }
            
        Quaternion desiredRotation = Quaternion.LookRotation(forwardVector, desiredUpVector);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * maxRotationRate);
        pole.localScale = Vector3.Lerp(pole.localScale, desiredLocalScale, Time.deltaTime * maxStretchRate);
        stopSignCenter.localPosition = Vector3.Lerp(stopSignCenter.localPosition, desiredStopSignPos, Time.deltaTime * maxStretchRate);

    }

    public void CollisionDetected(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Entity target))
        {
            audioSource.clip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.Play();

            target.Die(false, transform.forward * -killForce);
            //Destroy(this.gameObject, 2f);
            StartCoroutine(ChangeTextCoroutine());
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        CollisionDetected(other);
    }

    private IEnumerator ChangeTextCoroutine()
    {
        if (hitTexts.Length <= 0)
        {
            yield return null;
        }
        else
        {
            stopSignText.text = hitTexts[Random.Range(0, hitTexts.Length)];
            yield return new WaitForSeconds(textChangeDuration);
            stopSignText.text = defaultText;
        }
    }

}