using System.Collections.Generic;
using UnityEngine;

namespace UI.GraphicSettings
{
    internal static class GraphicSettingsUtils
    {
        public const bool APPLY_ON_START = true;
        private const string PLAYER_PREF_PREFIX = "graphics_";

        public static int GetGraphicSettingsValue(GraphicSetting graphicSetting)
        {
            string playerPrefKey = PLAYER_PREF_PREFIX + graphicSetting.ToString().ToLower();
            return PlayerPrefs.GetInt(playerPrefKey);
        }

        public static void SetGraphicSettingsValue(GraphicSetting graphicSetting, int value)
        {
            string playerPrefKey = PLAYER_PREF_PREFIX + graphicSetting.ToString().ToLower();
            PlayerPrefs.SetInt(playerPrefKey, value);
        }

        public static void ApplyGenericGraphicSettings()
        {
            GraphicSetting[] graphicSettings = System.Enum.GetValues(typeof(GraphicSetting)) as GraphicSetting[];
            List<GraphicSetting> genericSettings = new List<GraphicSetting>();
            List<GraphicSetting> worldSettings = GetWorldGraphicSettings();
            foreach (GraphicSetting graphicSetting in graphicSettings)
            {
                if (worldSettings.Contains(graphicSetting)) continue;
                genericSettings.Add(graphicSetting);
            }
            AppyGenericSettings(genericSettings);
            
        }

        /// <summary>
        /// Only applies graphic settings which are not world specific.
        /// </summary>
        private static void AppyGenericSettings(List<GraphicSetting> graphicSettings)
        {
            foreach (GraphicSetting graphicSetting in graphicSettings)
            {
                GraphicSettingManager graphicSettingManager = GraphicSettingFactory.GetManager(graphicSetting);
                if (graphicSettingManager == null)
                {
                    Debug.LogWarning($"Graphics Setting '{graphicSetting}' does not have a manager implemented");
                    continue;
                }

                int value = GetGraphicSettingsValue(graphicSetting);
                graphicSettingManager.ApplyGraphicSettings(value);
            }
        }

        private static List<GraphicSetting> GetWorldGraphicSettings()
        {
            return new List<GraphicSetting>
            {
                GraphicSetting.UIScale
            };
        }
        /// <summary>
        /// Only applies graphic settings that are world specific (ie do not apply in the title screen) such as Particles, UIScale, etc.
        /// </summary>
        public static void ApplyWorldGraphicSettings()
        {
            var worldGraphicSettings =  GetWorldGraphicSettings();
            AppyGenericSettings(worldGraphicSettings);
        }
    }
}