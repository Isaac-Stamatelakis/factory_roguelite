using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.ToolTip {
    public class ToolTipUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mText;
        public void setText(string text) {
            this.mText.text = text;
        }
    }
}

