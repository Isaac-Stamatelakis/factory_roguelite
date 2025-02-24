using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GeneralUIElements.Sliders
{
    public class FormattedSlider : MonoBehaviour
    {
        
        [SerializeField] private Scrollbar mScrollBar;
        [SerializeField] private RectTransform mHandleTransform;
        [SerializeField] private TextMeshProUGUI mNameText;
        [SerializeField] private TextMeshProUGUI mValueText;


        public void DisplayInteger(string title, int initial, int maxValue, int bonusDisplay, Action<int> valueChangeCallback)
        {
            mNameText.text = title;
            
            mScrollBar.onValueChanged.RemoveAllListeners();
            mScrollBar.value = (float)initial/maxValue;
            SetIntValueText(bonusDisplay, initial);
            mScrollBar.onValueChanged.AddListener((value) =>
            {
                // For some reason the max number of steps is 11 so have to hard code this
                int intValue = (int)(value * maxValue);
                valueChangeCallback(intValue);
                int step = GetStep(value, maxValue);
                SetAnchorStep(step, maxValue);
                SetIntValueText(bonusDisplay, step);
            });
        }

        private void SetIntValueText(int bonus, int value)
        {
            mValueText.text = (bonus+value).ToString();
        }

        private void SetFloatValueText(float bonus, float value)
        {
            mValueText.text = $"{(bonus+value):F1}";
        }
        private int GetStep(float value, int maxSteps)
        {
            return (int)((0.5d/maxSteps + value) * maxSteps);
        }

        private void SetAnchorStep(int step, int maxSteps)
        {
            if (maxSteps == 0) return;
            float size = (1 - mScrollBar.size) / (maxSteps - 1);
            mHandleTransform.pivot = new Vector2(step * size, (step + 1) * size);
        }
        public void DisplayFloat(string title, float initial, float maxValue, float bonusDisplay, Action<float> valueChangeCallback)
        {
            mNameText.text = title;
            mScrollBar.numberOfSteps = 0;
            mScrollBar.onValueChanged.RemoveAllListeners();
            mScrollBar.value = initial / maxValue;
            SetFloatValueText(bonusDisplay, initial);
            mScrollBar.onValueChanged.AddListener((value) =>
            {
                valueChangeCallback(value*maxValue);
                SetFloatValueText(bonusDisplay, value*maxValue);
            });
        }

        public void DisplayBool(string title, int initial, Action<int> valueChangeCallback, string falseText = "Off", string trueText = "On")
        {
            mNameText.text = $"{title}:";
            mScrollBar.numberOfSteps = 2;
            
            mScrollBar.onValueChanged.RemoveAllListeners();
            int val = initial == 0 ? 0 : 1;
            mScrollBar.value = val;
            DisplayBoolText(val, falseText, trueText);
            mScrollBar.size = 0.5f;
            mScrollBar.onValueChanged.AddListener((value) =>
            {
                int boolInt = value > 0.5f ? 1 : 0;
                valueChangeCallback(boolInt);
                DisplayBoolText(boolInt,falseText, trueText);
            });
        }

        private void DisplayBoolText(int value, string falseText, string trueText)
        {
            mValueText.text = value == 0 ? trueText : falseText;
        }
    }
}
