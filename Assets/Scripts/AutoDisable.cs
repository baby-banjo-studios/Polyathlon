using UnityEngine;

namespace BabyBanjo.Core.Utility
{
    public class AutoDisable : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}