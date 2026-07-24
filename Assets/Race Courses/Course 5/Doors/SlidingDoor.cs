using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [SerializeField] private float slideDistance = 1.88f;
    [SerializeField] private float slideSpeed = 2.5f;

    [SerializeField] private float closeDelay = 5f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen;

    private Coroutine moveCoroutine;
    private Coroutine closeCoroutine;

    void Start()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        leftOpenPos = leftClosedPos + Vector3.left * slideDistance;
        rightOpenPos = rightClosedPos + Vector3.right * slideDistance;
    }

    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        // Don't open for inanimate objects, slow-moving non-racers, or broken glass
        if (rb == null
            || rb.linearVelocity.magnitude <= 0.05f
            || (other.GetComponent<Racer>() == null && rb.linearVelocity.magnitude <= 0.5f)
            || other.gameObject.CompareTag("Dont KO Racer On Impact"))
            return;

        // Open the door if it isn't already.
        if (!isOpen)
        {
            isOpen = true;

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(
                SlideDoors(leftOpenPos, rightOpenPos));
        }

        // Every frame that something moves in the trigger, restart the close countdown
        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        isOpen = false;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(
            SlideDoors(leftClosedPos, rightClosedPos));
    }

    private IEnumerator SlideDoors(Vector3 leftTarget, Vector3 rightTarget)
    {
        Debug.Log("SlideDoors started");
        while (true)
        {
            leftDoor.localPosition = Vector3.MoveTowards(
                leftDoor.localPosition,
                leftTarget,
                slideSpeed * Time.deltaTime);

            rightDoor.localPosition = Vector3.MoveTowards(
                rightDoor.localPosition,
                rightTarget,
                slideSpeed * Time.deltaTime);

            bool leftDone = leftDoor.localPosition == leftTarget;
            bool rightDone = rightDoor.localPosition == rightTarget;

            if (leftDone && rightDone)
                yield break;

            yield return null;
        }
    }
}