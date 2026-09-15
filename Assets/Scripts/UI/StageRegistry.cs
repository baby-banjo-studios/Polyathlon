using BabyBanjo.Polyathlon.UI;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Race
{
    [CreateAssetMenu(fileName = "NewStage", menuName = "ScriptableObjects/StageRegistry")]
    public class StageRegistry : ScriptableObject, IPolypediaEntry
    {
        public string DisplayName { get => displayName; }
        public string Description { get => info; }
        public Sprite Thumbnail { get => icon; }
        public IList<Sprite> Slides { get => screenshots; }

        public string displayName;
        [TextArea]
        public string info;
        public Sprite icon;
        public List<Sprite> screenshots;
        public string sceneName;

    }
}