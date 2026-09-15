using BabyBanjo.Polyathlon.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public AudioClip[] songs;

        private AudioSource musicSource1;   // need 2 of these so we can fade
        private AudioSource musicSource2;
        private AudioSource soundSource;

        private int musicPlaying;   // index of song playing
        private bool isSource1 = true;   // reference the audiousource that is playing
        private bool switching = false;

        private PlayerController ourPlayer;

        public int MusicPlaying { get => musicPlaying; }

        // Start is called before the first frame update
        void Start()
        {
            // it is imperative that these 3 exist in the right order
            AudioSource[] sources = GetComponents<AudioSource>();
            musicSource1 = sources[0];
            musicSource2 = sources[1];
            soundSource = sources[2];

            PlaySong(0);
        }

        public void PlaySong(int idx)
        {
            if (idx < songs.Length)
            {
                AudioSource src = isSource1 ? musicSource1 : musicSource2;
                src.clip = songs[idx];
                src.loop = true;
                src.Play();
            }
            else
            {
                Debug.Log("No song is available at index " + idx + " of the songs array");
            }
        }

        /*  fades between the current song and the next one we have asked for */
        public IEnumerator SwitchSong(int idx, PlayerController currentPlayer)
        {
            // the first player to cross a music switch takes ownership of the music
            if (ourPlayer == null)
                ourPlayer = currentPlayer;
            // exit if we're already switching or this isn't our guy
            if (switching || ourPlayer != currentPlayer)
            {
                yield break;
            }
            switching = true;
            // update which source we're using
            isSource1 = !isSource1;
            musicPlaying = idx;
            PlaySong(idx);

            // get reference to old and new audiosources
            AudioSource old = isSource1 ? musicSource2 : musicSource1;
            AudioSource fresh = isSource1 ? musicSource1 : musicSource2;

            // get volume
            float vol = old.volume;
            float elapsedTime = 0;
            float duration = 2f;

            // fade
            while (elapsedTime < duration)
            {
                old.volume = Mathf.Lerp(vol, 0, elapsedTime / duration);
                fresh.volume = Mathf.Lerp(0, vol, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            old.Stop();
            switching = false;
        }
    }
}