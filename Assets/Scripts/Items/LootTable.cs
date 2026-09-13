using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "ScriptableObjects/LootTable")]
    public class LootTable : ScriptableObject
    {
        [System.Serializable]
        private class LootTableEntry
        {
            public ItemRegistry item;
            public AnimationCurve dropChance;
        }

        [SerializeField]
        private List<LootTableEntry> entries;

        public List<ItemRegistry> GetAllItems()
        {
            List<ItemRegistry> items = new List<ItemRegistry>();
            foreach (LootTableEntry entry in entries)
            {
                items.Add(entry.item);
            }
            return items;
        }

        public ItemRegistry GetItem(float percentWinning)
        {
            float totalWeight = 0f;
            foreach (LootTableEntry entry in entries)
            {
                totalWeight += entry.dropChance.Evaluate(percentWinning);
            }

            float randomWeight = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (LootTableEntry entry in entries)
            {
                cumulativeWeight += entry.dropChance.Evaluate(percentWinning);
                if (randomWeight <= cumulativeWeight)
                {
                    return entry.item;
                }
            }
            return entries[0].item;
        }
    }
}