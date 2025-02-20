using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI.Chat {
    public class TextChatMessageUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textUI;
        [SerializeField] private Image background;
        private TextChatUI textChatUI;
        private bool fade = true;
        private float displayTime;
        private readonly float startToFadeTime = 1.5f;
        public void Update() {
            if (!fade) {
                return;
            }
            displayTime -= Time.deltaTime;
            if (displayTime < startToFadeTime) {
                Color textColor = textUI.color;
                textColor.a = displayTime/startToFadeTime;
                textUI.color = textColor;
            }
            if (displayTime < 0) {
                GameObject.Destroy(gameObject);
            }
        }

        public void SetFade(bool fade)
        {
            this.fade = fade;
            if (!fade)
            {
                Color textColor = textUI.color;
                textColor.a = 1;
                textUI.color = textColor;
            }
        }

        public void SetBackground(bool enableBackground)
        {
            background.enabled = enableBackground;
        }
        public void destroy() {
            GameObject.Destroy(gameObject);
        }
        
        public void Initialize(string text,TextChatUI textChatUI, float displayTime, bool enableBackground) {
            textUI.text = text;
            this.textChatUI = textChatUI;
            this.displayTime = displayTime;
            background.enabled = enableBackground;
        }
    }
}

