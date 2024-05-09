using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI {
    public class DynamicTextCharacter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        public void setPosition(VerticalAlignmentOptions position) {
            text.verticalAlignment = position;
        }
        public void setColor(Color color) {
            text.color = color;
        }
        public void init(char c) {
            text.text = c.ToString();
            gameObject.name = c.ToString();
        }
        public static DynamicTextCharacter newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/General/DynamicText/Char").GetComponent<DynamicTextCharacter>();
        }
    }
}

