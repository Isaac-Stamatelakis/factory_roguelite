using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GraphicSettings
{
    public class GraphicSettingsUI : MonoBehaviour
    {
        [SerializeField] private Button mBackButton;
        [SerializeField] private Scrollbar mViewRangeScrollbar;
        [SerializeField] private Scrollbar mUIScaleScrollbar;
        [SerializeField] private Button mVSyncButton;
        [SerializeField] private Button mAntiAliasingButton;
        [SerializeField] private Button mParticleButton;
        [SerializeField] private Button mFullScreenButton;
        [SerializeField] private TMP_Dropdown mResolutionDropdown;
        
        private HashSet<GraphicSetting> settingsToApplyOnExit = new();
        private bool quitting;
        
        public void Start()
        {
            mBackButton.onClick.AddListener(CanvasController.Instance.PopStack);
            SetUpScrollbar(mViewRangeScrollbar, GraphicSetting.View_Range);
            SetUpScrollbar(mUIScaleScrollbar, GraphicSetting.UIScale);
            SetUpButton(mVSyncButton, GraphicSetting.VSync);
            SetUpButton(mAntiAliasingButton, GraphicSetting.AntiAliasing);
            SetUpButton(mParticleButton, GraphicSetting.Particles);
            SetUpButton(mFullScreenButton, GraphicSetting.FullScreen);
            SetUpDropDown(mResolutionDropdown, GraphicSetting.Resolution);
        }

        private void SetUpScrollbar(Scrollbar scrollbar, GraphicSetting graphicSetting)
        {
            int initial = GraphicSettingsUtils.GetGraphicSettingsValue(graphicSetting);
            TextMeshProUGUI textIndicator = scrollbar.GetComponentInChildren<TextMeshProUGUI>();
            textIndicator.text = GetText(graphicSetting, initial);
            GraphicSettingManager manager = GraphicSettingFactory.GetManager(graphicSetting);
            IScrollBarGraphicManager scrollBarGraphicManager = (IScrollBarGraphicManager)manager;
            scrollbar.value = (float)initial / (scrollBarGraphicManager.GetSteps()-1f);
            scrollbar.onValueChanged.AddListener((value) =>
            {
                int intVal = (int)(value * (scrollBarGraphicManager.GetSteps() - .5f));
                GraphicSettingsUtils.SetGraphicSettingsValue(graphicSetting, intVal);
                textIndicator.text = GetText(graphicSetting, intVal);
                OnValueChange(manager, graphicSetting, intVal);
            });
            
        }

        private void OnValueChange(GraphicSettingManager manager, GraphicSetting graphicSetting, int value)
        {
            if (manager is IDelayedExitGraphicManager)
            {
                settingsToApplyOnExit.Add(graphicSetting);
                
            } else if (manager is IDeselectDelayGraphicManager)
            {
                return;
            }
            else
            {
                manager?.ApplyGraphicSettings(value);
            }
        }
        
        private void SetUpDropDown(TMP_Dropdown dropdown, GraphicSetting graphicSetting)
        {
            int initial = GraphicSettingsUtils.GetGraphicSettingsValue(graphicSetting);
            dropdown.value = initial;
            GraphicSettingManager manager = GraphicSettingFactory.GetManager(graphicSetting);
            if (manager is not IDropDownGraphicManager dropDownGraphicManager)
            {
                Debug.LogError($"Graphic Setting {graphicSetting} manager is not IDropDownGraphicManager");
                return;
            }

            List<string> strings = dropDownGraphicManager.GetStringValues();
            for (var index = 0; index < strings.Count; index++)
            {
                var str = strings[index];
                strings[index] = $"{graphicSetting}:{str}";
            }

            dropdown.options = GlobalHelper.StringListToDropDown(strings);
            
            dropdown.onValueChanged.AddListener((value) =>
            {
                GraphicSettingsUtils.SetGraphicSettingsValue(graphicSetting, value);
                OnValueChange(manager, graphicSetting, value);
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
                
                textElement.text = GetText(graphicSetting,value);
                OnValueChange(manager, graphicSetting, value);
            });
        }

        private string GetText(GraphicSetting graphicSetting, int value)
        {
            GraphicSettingManager manager = GraphicSettingFactory.GetManager(graphicSetting);
            string valueText = manager == null ? $"?{value}?" : manager.GetValueName(value);
            return $"{graphicSetting.ToString().Replace("_", " ")}:{valueText}";
        }

        public void OnApplicationQuit()
        {
            quitting = true;
        }

        public void OnDestroy()
        {
            if (quitting || !Application.isPlaying) return;
            
            foreach (GraphicSetting setting in settingsToApplyOnExit)
            {
                var manager = GraphicSettingFactory.GetManager(setting);
                int value = GraphicSettingsUtils.GetGraphicSettingsValue(setting);
                manager?.ApplyGraphicSettings(value);
            }
        }
    }
}
