using BabyBanjo.Core.Input;
using TMPro;
using UnityEngine;

namespace BabyBanjo.Polyathlon.UI
{
    public class PlayerReadyIndicator : MonoBehaviour
    {
        public TextMeshProUGUI playerLabel;
        public TextMeshProUGUI readyLabel;

        private int playerIdx;
        private ControlScheme scheme;

        public bool IsFree { get; private set; }

        private const string PRE_JOINED_TEXT = "Press Space/Start to join";

        public void Initialize(int playerIdx)
        {
            playerLabel.text = PRE_JOINED_TEXT;
            this.playerIdx = playerIdx;
            readyLabel.text = "";
            IsFree = true;
        }

        public void AddPlayer(ControlScheme scheme)
        {
            this.scheme = scheme;
            playerLabel.text = string.Format("Player {0}", playerIdx);
            IsFree = false;
            Unready();
        }

        public void RemovePlayer()
        {
            playerLabel.text = PRE_JOINED_TEXT;
            readyLabel.text = "";
            IsFree = true;
        }

        public void Ready()
        {
            readyLabel.text = "Ready";
        }

        public void Unready()
        {
            readyLabel.text = GetNotReadyText();
        }

        private string GetNotReadyText()
        {
            return string.Format("Press {0} to ready up", scheme == ControlScheme.Keyboard ? "Space" : "A");
        }
    }
}