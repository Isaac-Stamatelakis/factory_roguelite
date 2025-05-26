using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.GraphicSettings.Managers
{
    internal class UIScaleGraphicSettingManager : GraphicSettingManager, IScrollBarGraphicManager, IDelayedExitGraphicManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            UIScale uiScale = (UIScale)value;
            float scaleValue = GetUIScaleValue(uiScale);
            GameObject[] scalableUIElements = GameObject.FindGameObjectsWithTag("ScalableUIElement");
            foreach (GameObject scalableUIElement in scalableUIElements)
            {
                scalableUIElement.transform.localScale =  new Vector3(scaleValue, scaleValue, scaleValue);
            }
        }

        public void ApplyDefaultScale(GameObject uiElement)
        {
            ApplyScale(uiElement,GraphicSettingsUtils.GetGraphicSettingsValue(GraphicSetting.UIScale));
        }

        public void ApplyScale(GameObject uiElement, int value)
        {
            UIScale uiScale = (UIScale)value;
            float scaleValue = GetUIScaleValue(uiScale);
            uiElement.transform.localScale =  new Vector3(scaleValue, scaleValue, scaleValue);
        }

        private float GetUIScaleValue(UIScale scale)
        {
            return scale switch
            {
                UIScale.Huge => 1f,
                UIScale.Large => .9f,
                UIScale.Normal => 0.8f,
                UIScale.Small => 0.7f,
                UIScale.Tiny => 0.6f,
                _ => 1f
            };
        }

        public override string GetValueName(int value)
        {
            UIScale uiScale = (UIScale)value;
            return uiScale.ToString();
        }

        public int GetSteps()
        {
            return System.Enum.GetValues(typeof(UIScale)).Length;
        }

        private enum UIScale
        {
            Tiny = 0,
            Small = 1,
            Normal = 2,
            Large = 3,
            Huge = 4
        }
    }
}