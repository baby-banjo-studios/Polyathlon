using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyBanjo.Polyathlon.Audio
{
    public class MenuMusic : MonoBehaviour
    {
        public AudioClip opening;
        public AudioClip loop;

        private AudioSource audioSource;
        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            StartCoroutine(WaitForLoop());
        }

        private IEnumerator WaitForLoop()
        {
            audioSource.clip = opening;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
            audioSource.clip = loop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}