using UnityEngine;

namespace UI.GraphicSettings.Managers.UIScale
{
    public class ScaleSyncUI : MonoBehaviour
    {
        void Start()
        {
            UIScaleGraphicSettingManager uiManager = (UIScaleGraphicSettingManager)GraphicSettingFactory.GetManager(GraphicSetting.UIScale);
            uiManager.ApplyDefaultScale(gameObject);
        }
    }
}
