using BabyBanjo.Polyathlon.Entities;
using BabyBanjo.Polyathlon.Race;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class MysteryItem : Item
    {
        public LootTable lootTable;
        public ItemRegistry singleItemDrop;
        public float lift = 1f;
        public float liftTime = 3f;
        private Transform parentTransform;
        public AnimationCurve animationCurve;
        public float rotationRate = 30f;

        private float startingY;

        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
            int place = RaceManager.GetPosition(racer);
            int numRacers = RaceManager.GetListOfRacers().Count;
            int distanceFromLast = (numRacers - 1) - (place - 1);

            if (lootTable == null)
            {
                lootTable = RaceManager.CurrLootTable;
            }

            float percentWinning = (float)distanceFromLast / numRacers;
            if (lootTable != null)
            {
                ItemRegistry item = lootTable.GetItem(percentWinning);
                item.itemPrefab.Pickup(racer);
            }
            else if (singleItemDrop != null)
            {
                singleItemDrop.itemPrefab.Pickup(racer);
            }
        }

        protected override void Start()
        {
            base.Start();
            startingY = transform.localPosition.y;
            parentTransform = transform.parent;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationRate * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, (parentTransform == null ? startingY : parentTransform.position.y + startingY) + lift * animationCurve.Evaluate(Time.time % animationCurve.keys[animationCurve.length - 1].time), transform.position.z);
        }

    }
}