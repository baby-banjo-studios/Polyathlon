using BabyBanjo.Core.Input;
using TMPro;
using UnityEngine;

namespace BabyBanjo.Polyathlon.UI
{
    public class AllReadyOverlay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;
        [SerializeField]
        private string topText;

        public void SetActive(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        public void SetControlScheme(ControlScheme scheme)
        {
            string buttonToPress = "[button]";
            if (scheme == ControlScheme.Keyboard)
            {
                buttonToPress = "Enter";
            }
            else if (scheme == ControlScheme.Gamepad)
            {
                buttonToPress = "Start";
            }
            string prependText = topText;
            if (topText != "")
            {
                prependText += '\n';
            }
            text.text = string.Format("{0}Press {1} to continue", prependText, buttonToPress);
        }
    }
}