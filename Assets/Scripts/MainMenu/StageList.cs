using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Race
{
    [CreateAssetMenu(fileName = "NewStageList", menuName = "ScriptableObjects/StageList")]
    public class StageList : ScriptableObject
    {
        public List<StageRegistry> stages;
    }
}