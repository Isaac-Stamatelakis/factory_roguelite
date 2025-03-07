using UnityEngine;

namespace UI.GraphicSettings.Managers
{
    internal class VSyncGraphicManager : BooleanGraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            QualitySettings.vSyncCount = value;
        }
    }
}