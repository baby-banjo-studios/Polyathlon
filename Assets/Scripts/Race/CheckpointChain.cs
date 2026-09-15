using UnityEngine;

namespace BabyBanjo.Polyathlon.Race
{
    public class CheckpointChain : MonoBehaviour
    {
        private Checkpoint[] checkpoints;

        private void Awake()
        {
            checkpoints = GetComponentsInChildren<Checkpoint>();
            for (int i = checkpoints.Length - 1; i >= 0; i--)
            {
                checkpoints[i].seq = i;
                if (i < checkpoints.Length - 1)
                {
                    checkpoints[i].next = checkpoints[i + 1];
                    checkpoints[i].distance = Vector3.Distance(checkpoints[i].transform.position, checkpoints[i + 1].transform.position);
                }
            }
        }

        public Checkpoint GetFirstCheckpoint()
        {
            return checkpoints[0];
        }
    }
}