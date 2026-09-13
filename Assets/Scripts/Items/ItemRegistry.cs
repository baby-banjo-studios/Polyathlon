using BabyBanjo.Polyathlon.UI;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    [CreateAssetMenu(fileName = "ItemRegistry", menuName = "ScriptableObjects/ItemRegistry")]
    public class ItemRegistry : ScriptableObject, IPolypediaEntry
    {
        public string DisplayName { get => displayName; }
        public string Description { get => description; }
        public Sprite Thumbnail { get => icon; }
        public IList<Sprite> Slides { get => screenshots; }

        public string displayName;
        [TextArea]
        public string description;
        public Sprite icon;
        public List<Sprite> screenshots;
        public Item itemPrefab;

    }
}