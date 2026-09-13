using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BabyBanjo.Core.UI
{
    public class Selector : MonoBehaviour
    {
        protected Color color;
        [SerializeField]
        protected float moveDuration_sec = 0.05f;
        protected bool isMoving = false;
        private Coroutine activeCoroutine = null;

        public GridEntry selectedEntry;
        protected RectTransform target;
        [SerializeField]
        protected TextMeshProUGUI label;
        public int SelectorIndex { get; protected set; }
        public bool Locked { get; protected set; }
        public bool Active { get => gameObject.activeInHierarchy; }
        public bool Moving { get => isMoving; }

        public void Initialize(int selectorIndex, string labelText)
        {
            SelectorIndex = selectorIndex;
            label.text = labelText;
        }

        public void SetActive(bool val)
        {
            gameObject.SetActive(val);
        }

        public void MoveToTarget(RectTransform newTarget, bool warp)
        {
            if (warp || !gameObject.activeInHierarchy)
            {
                target = newTarget;
                transform.position = newTarget.position;
            }
            else if (!isMoving)
            {
                isMoving = true;
                target = newTarget;
                activeCoroutine = StartCoroutine(MoveToTargetCoroutine());
            }
        }

        public void InterruptMove()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                isMoving = false;
                activeCoroutine = null;
            }
        }

        public void Lock()
        {
            Locked = true;
        }

        public void Unlock()
        {
            Locked = false;
        }

        protected IEnumerator MoveToTargetCoroutine()
        {
            //yield return null;
            float elapsedTime = 0;
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;
            while (elapsedTime < moveDuration_sec)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, target.position, elapsedTime / moveDuration_sec);
                //Debug.Log(string.Format("moved to {0},{1},{2}", transform.position.x, transform.position.y, transform.position.z));
                yield return null;
            }
            transform.position = target.position;
            isMoving = false;
            activeCoroutine = null;
        }
    }
}