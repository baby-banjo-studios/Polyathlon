using System;
using TMPro;
using UnityEngine;

namespace BabyBanjo.Polyathlon.UI
{
    public class CreditsUI : BaseMenuUI
    {
        [SerializeField]
        protected TextMeshProUGUI yearsText;

        protected override void Awake()
        {
            base.Awake();
            yearsText.text = string.Format("2020 - {0}", DateTime.Now.Year);
        }
    }
}