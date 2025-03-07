using System.Collections.Generic;
using UnityEngine;

namespace UI.GraphicSettings.Managers
{
    internal class ResolutionManager : GraphicSettingManager, IDropDownGraphicManager
    {
        private List<Vector2Int> screenResolutions = new List<Vector2Int>
        {
            new Vector2Int(1280, 720),    // HD
            new Vector2Int(1920, 1080),   // Full HD
            new Vector2Int(2560, 1440),   // QHD
            new Vector2Int(3840, 2160),   // 4K UHD
            new Vector2Int(1366, 768),    // WXGA
        };
        public override void ApplyGraphicSettings(int value)
        {
            Vector2Int resolution = GetScreenResolution(value);
            Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
        }

        public override string GetValueName(int value)
        {
            Vector2Int resolution = GetScreenResolution(value);
            return $"{resolution.x}x{resolution.y}";
        }

        private Vector2Int GetScreenResolution(int value)
        {
            if (value < 0 || value >= screenResolutions.Count)
            {
                Debug.LogWarning("Out of bounds screen resolution setting");
                return screenResolutions[0];
            }
            return screenResolutions[value];
        }


        public List<string> GetStringValues()
        {
            List<string> strings = new List<string>();
            for (int i = 0; i < screenResolutions.Count; i++)
            {
                string ValueName = GetValueName(i);
                strings.Add(ValueName);
            }
            return strings;
        }
    }
}