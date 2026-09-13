using BabyBanjo.Polyathlon.Entities;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public class BackSentryItem : Item
    {
        private bool used = false;
        public override void Pickup(Racer racer)
        {
            base.Pickup(racer);
        }

        public override void Use(Racer racer)
        {
            //if (!used)
            {
                //used = true;
                BackSentryObject sentryObj = racer.BackpackMount.backSentry;
                //sentryObj.target = RaceManager.GetClosestRacerAheadOfThisOne(racer);
                sentryObj.Initialize(racer);

                racer.EquipItem(null);
            }
        }
    }
}