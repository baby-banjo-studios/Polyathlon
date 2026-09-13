using BabyBanjo.Polyathlon.UI;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Entities
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "ScriptableObjects/CharacterRegistry")]
    public class CharacterRegistry : ScriptableObject, IPolypediaEntry
    {
        public string DisplayName { get => displayName; }
        public string Description { get => subtitle; }
        public Sprite Thumbnail { get => icon; }
        public IList<Sprite> Slides { get => screenshots; }

        public string displayName;
        public string subtitle;
        public Sprite icon;
        public List<Sprite> screenshots;
        //public GameObject model;    
        public GameObject playerObj;
        public GameObject npcObj;
        public GameObject previewObj;

    }
}