using UnityEngine;
using System.Collections;

namespace BabyBanjo.Polyathlon.Race
{
    public class MelonButton : MonoBehaviour
    {
        public Transform building;
        public Transform barrier;
        public float duration = 1f;

        public IEnumerator Activate()
        {
            Vector3 startPos = building.position;
            Vector3 endPos = new Vector3(startPos.x, startPos.y - 20f, startPos.z);
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                building.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            startPos = barrier.position;
            endPos = new Vector3(startPos.x, startPos.y - 5f, startPos.z);
            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                barrier.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            barrier.gameObject.SetActive(false);
        }
    }
}