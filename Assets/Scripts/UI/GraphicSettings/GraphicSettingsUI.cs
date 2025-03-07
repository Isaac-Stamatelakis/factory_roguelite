using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable InconsistentNaming

namespace UI.GraphicSettings
{
    public class GraphicSettingsUI : MonoBehaviour
    {
        [SerializeField] private Scrollbar mViewRangeScrollbar;
        [SerializeField] private Button mVSyncButton;
        [SerializeField] private Button mAntiAliasingButton;
        [SerializeField] private Button mParticleButton;
        [SerializeField] private Button mFullScreenButton;
        [SerializeField] private TMP_Dropdown mResolutionDropdown;
        
        public void Initialize()
        {
            SetUpScrollbar(mViewRangeScrollbar, GraphicSetting.View_Range);
            SetUpButton(mVSyncButton, GraphicSetting.VSync);
            SetUpButton(mAntiAliasingButton, GraphicSetting.AntiAliasing);
            SetUpButton(mParticleButton, GraphicSetting.Particles);
            SetUpButton(mFullScreenButton, GraphicSetting.FullScreen);

        }

        private void SetUpScrollbar(Scrollbar scrollbar, GraphicSetting graphicSetting)
        {
            int initial = GraphicSettingsUtils.GetGraphicSettingsValue(graphicSetting);
            TextMeshProUGUI textIndicator = scrollbar.GetComponentInChildren<TextMeshProUGUI>();
            textIndicator.text = GetText(graphicSetting, initial);
            mViewRangeScrollbar.onValueChanged.AddListener((value) =>
            {
                int intVal = (int)(value * (mViewRangeScrollbar.numberOfSteps - 1));
                GraphicSettingsUtils.SetGraphicSettingsValue(graphicSetting, intVal);
                GraphicSettingManager manager = GraphicSettingFactory.GetManager(graphicSetting);
                textIndicator.text = GetText(graphicSetting, intVal);
            });
        }

        private void SetUpButton(Button button, GraphicSetting graphicSetting)
        {
            TextMeshProUGUI textElement = button.GetComponentInChildren<TextMeshProUGUI>();
            int initial = GraphicSettingsUtils.GetGraphicSettingsValue(graphicSetting);
            textElement.text = GetText(graphicSetting,initial);
            
            button.onClick.AddListener(() =>
            {
                int value = GraphicSettingsUtils.GetGraphicSettingsValue(graphicSetting);
                value = value == 0 ? 1 : 0;
                GraphicSettingsUtils.SetGraphicSettingsValue(graphicSetting,value);
                GraphicSettingManager manager = GraphicSettingFactory.GetManager(graphicSetting);
                manager?.ApplyGraphicSettings(value);
            });
        }

        private string GetText(GraphicSetting graphicSetting, int value)
        {
            GraphicSettingManager manager = GraphicSettingFactory.GetManager(graphicSetting);
            string valueText = manager == null ? $"?{value}?" : manager.GetValueName(value);
            return $"{graphicSetting.ToString().Replace("_", " ")}:{valueText}";
        }
    }

    internal static class GraphicSettingsUtils
    {
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

    internal static class GraphicSettingFactory
    {
        internal static GraphicSettingManager GetManager(GraphicSetting graphicSetting)
        {
            switch (graphicSetting)
            {
                case GraphicSetting.View_Range:
                    return new ViewRangeGraphicManager();
                case GraphicSetting.VSync:
                    return new VSyncGraphicManager();
                case GraphicSetting.AntiAliasing:
                    return new AntiAliasingManager();
                case GraphicSetting.Particles:
                    break;
                case GraphicSetting.FullScreen:
                    return new FullScreenManager();
                case GraphicSetting.Resolution:
                    return new ResolutionManager();
                default:
                    return null;
            }
            return null;
        }
    }
    internal enum GraphicSetting
    {
        View_Range,
        VSync,
        AntiAliasing,
        Particles,
        FullScreen,
        Resolution
    }
    internal abstract class GraphicSettingManager
    {
        public abstract void ApplyGraphicSettings(int value);
        public abstract string GetValueName(int value);
    }

    internal abstract class BooleanGraphicSettingManager : GraphicSettingManager
    {
        public override string GetValueName(int value)
        {
            return value == 0 ? "Off" : "On";
        }
    }

    internal class VSyncGraphicManager : BooleanGraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            QualitySettings.vSyncCount = value;
        }
    }
    
    internal class AntiAliasingManager : BooleanGraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            QualitySettings.antiAliasing = value;
        }
    }
    
    internal class FullScreenManager : BooleanGraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            Screen.fullScreen = value != 0;
        }
    }

    internal class ResolutionManager : GraphicSettingManager
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
    }

    internal class ViewRangeGraphicManager : GraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            CameraViewSize cameraViewSize = (CameraViewSize)value;
            Camera camera = Camera.main;
            if (!camera) return;
            CameraView cameraView = camera.GetComponent<CameraView>();
            if (!cameraView) return;
            cameraView.SetViewRange(cameraViewSize);
        }

        public override string GetValueName(int value)
        {
            CameraViewSize cameraViewSize = (CameraViewSize)value;
            return cameraViewSize.ToString();
        }
    }
    
}
