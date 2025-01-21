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
        private float baseBackgroundAlpha;
        public void Update() {
            if (!fade) {
                return;
            }
            Color baseBackgroundColor = background.color;
            baseBackgroundColor.a *= startToFadeTime/displayTime;
            displayTime -= Time.deltaTime;
            if (displayTime < startToFadeTime) {
                Color textColor = textUI.color;
                textColor.a = displayTime/startToFadeTime;
                textUI.color = textColor;

                Color backgroundColor = background.color;
                backgroundColor.a = baseBackgroundAlpha * displayTime/startToFadeTime;
                background.color = backgroundColor;
            }
            if (displayTime < 0) {
                GameObject.Destroy(gameObject);
            }
        }
        public void destroy() {
            GameObject.Destroy(gameObject);
        }
        public void setFade(bool fade) {
            this.fade = fade;
            if (!fade) {
                Color textColor = textUI.color;
                textColor.a = 1;
                textUI.color = textColor;

                Color backgroundColor = background.color;
                backgroundColor.a = baseBackgroundAlpha;
                background.color = backgroundColor;
            }
        }
        public void init(string text,TextChatUI textChatUI, float displayTime) {
            textUI.text = text;
            this.textChatUI = textChatUI;
            this.displayTime = displayTime;
            baseBackgroundAlpha = background.color.a;
        }
    }
}

