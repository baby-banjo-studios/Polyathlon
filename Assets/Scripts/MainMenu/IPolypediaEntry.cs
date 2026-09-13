using System.Collections.Generic;
using UnityEngine;
namespace BabyBanjo.Polyathlon.UI
{
    public interface IPolypediaEntry
    {
        string DisplayName { get; }
        string Description { get; }
        Sprite Thumbnail { get; }
        IList<Sprite> Slides { get; }
    }
}