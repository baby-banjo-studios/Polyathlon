using UnityEngine;

namespace BabyBanjo.Polyathlon.Items
{
    public enum BackpackOptions
    {
        Nothing,
        Jetpack,
        Glider,
        BackSentry
    }

    public class BackpackMount : MonoBehaviour
    {
        public GameObject jetpack;
        public GameObject glider;
        public BackSentryObject backSentry;

        public void Equip(BackpackOptions item)
        {
            switch (item)
            {
                case BackpackOptions.Jetpack:
                    {
                        jetpack.SetActive(true);
                    }
                    break;
                case BackpackOptions.Glider:
                    {
                        glider.SetActive(true);
                    }
                    break;
                case BackpackOptions.BackSentry:
                    {
                        backSentry.gameObject.SetActive(true);
                    }
                    break;
            }
        }

        public void Unequip(BackpackOptions item)
        {

            switch (item)
            {
                case BackpackOptions.Jetpack:
                    {
                        jetpack.SetActive(false);
                    }
                    break;
                case BackpackOptions.Glider:
                    {
                        glider.SetActive(false);
                    }
                    break;
                case BackpackOptions.BackSentry:
                    {
                        backSentry.gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }
}