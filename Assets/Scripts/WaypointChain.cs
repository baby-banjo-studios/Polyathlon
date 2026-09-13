using UnityEngine;

namespace BabyBanjo.Polyathlon.Race
{
    public class WaypointChain : MonoBehaviour
    {
        private IWaypointable[] waypoints;

        private void Awake()
        {
            waypoints = GetComponentsInChildren<IWaypointable>();
            for (int i = waypoints.Length - 1; i >= 0; i--)
            {
                waypoints[i].Seq = i;
                if (i < waypoints.Length - 1)
                    waypoints[i].Next = waypoints[i + 1];
            }
        }

        public IWaypointable GetStartingWaypoint()
        {
            return waypoints[0];
        }
    }
}