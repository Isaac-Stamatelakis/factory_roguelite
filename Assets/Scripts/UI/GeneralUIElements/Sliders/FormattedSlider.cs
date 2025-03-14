using System;
using Robot.Upgrades.Info;
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


        public void DisplayInteger(string title, int initial, int maxValue, Action<int> valueChangeCallback, IDiscreteUpgradeAmountFormatter amountFormatter)
        {
            mNameText.text = $"{title}:";
            mScrollBar.numberOfSteps = 0;
            mScrollBar.onValueChanged.RemoveAllListeners();
            mScrollBar.value = (float)initial/maxValue;
            SetIntValueText(initial,amountFormatter);
            mScrollBar.onValueChanged.AddListener((value) =>
            {
                // For some reason the max number of steps is 11 so have to hard code this
                int intValue = (int)(value * maxValue);
                valueChangeCallback(intValue);
                int step = GetStep(value, maxValue);
                SetAnchorStep(step, maxValue);
                SetIntValueText(step,amountFormatter);
            });
        }

        private void SetIntValueText(int value, IDiscreteUpgradeAmountFormatter amountFormatter)
        {
            mValueText.text = amountFormatter == null ? value.ToString() : amountFormatter.Format(value);
        }

        private void SetFloatValueText(float value, IContinousUpgradeAmountFormatter amountFormatter)
        {
            mValueText.text = amountFormatter == null ? $"{value:F1}" : amountFormatter.Format(value);
        }
        private int GetStep(float value, int maxSteps)
        {
            return (int)((0.5d/(maxSteps-1) + value) * (maxSteps));
        }

        private void SetAnchorStep(int step, int maxSteps)
        {
            if (maxSteps == 0) return;
            float size = (1 - mScrollBar.size) / (maxSteps - 1);
            mHandleTransform.pivot = new Vector2(step * size, (step + 1) * size);
        }
        public void DisplayFloat(string title, float initial, float maxValue, Action<float> valueChangeCallback,IContinousUpgradeAmountFormatter amountFormatter)
        {
            mNameText.text = $"{title}:";
            mScrollBar.numberOfSteps = 0;
            mScrollBar.onValueChanged.RemoveAllListeners();
            mScrollBar.value = initial / maxValue;
            SetFloatValueText(initial, amountFormatter);
            mScrollBar.onValueChanged.AddListener((value) =>
            {
                valueChangeCallback(value*maxValue);
                SetFloatValueText(value*maxValue, amountFormatter);
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
            mValueText.text = value == 0 ? falseText : trueText;
        }
    }
}
