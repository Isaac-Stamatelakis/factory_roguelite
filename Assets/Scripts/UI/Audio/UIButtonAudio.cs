using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Audio
{
    public class UIButtonAudio : MonoBehaviour
    {
        public void Start()
        {
            Button button = GetComponent<Button>();
            if (!button) return;
            
            button.onClick.AddListener(() =>
            {
                CanvasController.Instance.PlayAudioClip(UIAudioClipType.Button);
            });
        }
    }
}
