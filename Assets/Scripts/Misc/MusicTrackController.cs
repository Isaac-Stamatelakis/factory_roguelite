using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc.Audio {
    public class MusicTrackController : MonoBehaviour
    {
        private static MusicTrackController instance;
        [SerializeField] private List<AudioClip> songs;
        private int currentIndex;
        private AudioSource audioSource;
        private AudioClip defaultSong;

        public static MusicTrackController Instance { get => instance;}

        public void Awake() {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            defaultSong = audioSource.clip;
        }
        public void FixedUpdate() {
            if (!audioSource.isPlaying) {
                currentIndex ++;
                currentIndex = currentIndex % songs.Count;
                audioSource.clip = songs[currentIndex];
                audioSource.Play();
            }
        }
        public void SetSong(List<AudioClip> songs) {
            if (songs.Count == 0) {
                return;
            }
            this.songs = songs;
            audioSource.loop = songs.Count==1;
            audioSource.clip = songs[0];
            audioSource.Play();
            currentIndex = 0;
        }

        public void RestoreDefaultSong()
        {
            if (ReferenceEquals(audioSource.clip, defaultSong)) return;
            audioSource.clip = defaultSong;
            audioSource.Play();
            
        }
    }
}

