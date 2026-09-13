using UnityEngine;

namespace BabyBanjo.Polyathlon.Entities
{
    public class DebugCharacterSwitcher : MonoBehaviour
    {
        private int playerIdx = 0;
        private PlayerController[] players;
        private bool playerChangedThisFrame = false;

        private void Awake()
        {
            players = GetComponentsInChildren<PlayerController>(true);
        }

        private void Start()
        {
            foreach (PlayerController player in players)
            {
                player.EnableDebugControls();
            }
            SetPlayer(playerIdx);
        }

        private void Update()
        {
            playerChangedThisFrame = false;
        }

        public void NextChar()
        {
            if (!playerChangedThisFrame)
            {
                playerIdx++;
                if (playerIdx > players.Length)
                {
                    playerIdx = 0;
                }
                SetPlayer(playerIdx);
            }
        }

        public void PrevChar()
        {
            if (!playerChangedThisFrame)
            {
                playerIdx--;
                if (playerIdx < 0)
                {
                    playerIdx = players.Length - 1;
                }
                SetPlayer(playerIdx);
            }
        }

        private void SetPlayer(int idx)
        {
            playerChangedThisFrame = true;
            for (int i = 0; i < players.Length; i++)
            {
                players[i].gameObject.SetActive(false);
            }
            players[idx].gameObject.SetActive(true);
        }
    }
}