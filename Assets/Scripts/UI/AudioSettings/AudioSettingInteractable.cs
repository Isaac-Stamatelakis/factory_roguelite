using UnityEngine;

namespace UI.AudioSettings
{
    public class AudioSettingInteractable : MonoBehaviour
    {
        private AudioSetting? audioSetting;

        public void SetAudioSetting(AudioSetting audioSetting)
        {
            this.audioSetting = audioSetting;
            float val = AudioSettingUtils.GetAudioSetting(audioSetting);
            SetAudioLevel(val);
        }

        public AudioSetting? GetAudioSetting()
        {
            return audioSetting;
        }

        public void SetAudioLevel(float val)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (!audioSource) return;
            audioSource.volume = val;
        }
    }
}
