using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Race
{
    public interface IWaypointable
    {
        IWaypointable Next { get; set; }
        int Seq { get; set; }
        Vector3 GetPos(NPC npc);
        float GetHeight();
        IWaypointable[] GetFork();
    }
}