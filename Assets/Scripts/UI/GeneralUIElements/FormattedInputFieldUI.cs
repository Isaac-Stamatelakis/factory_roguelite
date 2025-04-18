using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace UI.GeneralUIElements
{
    public class FormattedInputFieldUI : MonoBehaviour
    {
    
        [SerializeField] private TMP_InputField mInputField;
        [SerializeField] private TextMeshProUGUI mTitleText;
        public TMP_InputField InputField => mInputField;


        public void DisplayUInt(string title, uint value, Action<uint> callback)
        {
            mInputField.text = value.ToString();
            mTitleText.text = title;
            mInputField.onValueChanged.AddListener((text) =>
            {
                value = uint.TryParse(text, out uint result) ? result : 0;
                callback(value);
            });
        }
        
        public void DisplayULong(string title, ulong value, Action<ulong> callback)
        {
            mInputField.text = value.ToString();
            mTitleText.text = title;
            mInputField.onValueChanged.AddListener((text) =>
            {
                value = ulong.TryParse(text, out ulong result) ? result : 0;
                callback(value);
            });
        }
    
        public void DisplayFloat(string title, float value, Action<float> callback, float min = float.MinValue, float max = float.MaxValue)
        {
            mInputField.text = $"{value:F2}";
            mTitleText.text = title;
            mInputField.onValueChanged.AddListener((text) =>
            {
                value = float.TryParse(text, out float result) ? result : 0;
                if (value < min) value = min;
                if (value > max) value = max;
                mInputField.text = $"{value:F2}";
                callback(value);
            });
        }
    
        public void DisplayString(string title, string value, Action<string> callback)
        {
            mInputField.text = value;
            mTitleText.text = title;
            mInputField.onValueChanged.AddListener((text) =>
            {
                callback(text);
            });
        }
    }
}
