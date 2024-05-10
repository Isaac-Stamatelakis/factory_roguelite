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
        private bool fade;
        private float displayTime;
        private readonly float startToFadeTime = 1.5f;
        private float baseBackgroundAlpha;
        public void FixedUpdate() {
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
        public void init(string text, bool fade, float displayTime) {
            textUI.text = text;
            this.fade = fade;
            this.displayTime = displayTime;
            baseBackgroundAlpha = background.color.a;
        }
    }
}

