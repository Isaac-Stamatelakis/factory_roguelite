using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.GraphicSettings.Managers
{
    internal class UIScaleGraphicSettingManager : GraphicSettingManager, IScrollBarGraphicManager
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

        private float GetUIScaleValue(UIScale scale)
        {
            return scale switch
            {
                UIScale.Large => 1f,
                UIScale.Normal => 0.9f,
                UIScale.Small => 0.8f,
                UIScale.Tiny => 0.7f,
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
            Large = 0,
            Normal = 1,
            Small = 2,
            Tiny = 3
        }
    }
}