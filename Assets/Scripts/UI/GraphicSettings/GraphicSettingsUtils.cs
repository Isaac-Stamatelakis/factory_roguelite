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

        public static void ApplyAllGraphicsSettings()
        {
            GraphicSetting[] graphicSettings = System.Enum.GetValues(typeof(GraphicSetting)) as GraphicSetting[];
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
    }
}