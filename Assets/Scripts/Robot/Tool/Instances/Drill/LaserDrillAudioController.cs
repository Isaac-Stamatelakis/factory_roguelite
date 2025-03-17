using System;
using UI.AudioSettings;
using UnityEngine;

namespace Robot.Tool.Instances.Drill
{
    public enum TileAudioType
    {
        None = 0,
        Stone = 1,
        Ore = 2,
        Metal = 3,
    }
    public class LaserDrillAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip StoneClip;
        [SerializeField] private AudioClip OreClip;
        [SerializeField] private AudioClip MetaClip;
        private bool queued = false;

        public void Start()
        {
            float initialVolume = audioSource.volume;
            float tileVolume = AudioSettingUtils.GetAudioSetting(AudioSetting.Tiles);
            audioSource.volume = initialVolume * tileVolume;
        }

        private AudioClip GetAudioClip(TileAudioType audioType)
        {
            switch (audioType)
            {
                case TileAudioType.None:
                    return null;
                case TileAudioType.Stone:
                    return StoneClip;
                case TileAudioType.Ore:
                    return OreClip;
                case TileAudioType.Metal:
                    return MetaClip;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioType), audioType, null);
            }
        }
        public void PlayAudioClip(TileAudioType audioType)
        {
            AudioClip audioClip = GetAudioClip(audioType);
            audioSource.clip = audioClip;
            if (!audioClip)
            {
                audioSource.Stop();
                return;
            }
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            
            if (audioSource.isPlaying)
            {
                queued = true;
                return;
            }
            audioSource.PlayOneShot(audioClip);
        }

        public void Update()
        {
            if (!queued) return;
            if (audioSource.isPlaying) return;
            audioSource.Play();
            queued = false;
        }
    }
}
