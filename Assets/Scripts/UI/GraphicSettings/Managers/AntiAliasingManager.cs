using UnityEngine;

namespace UI.GraphicSettings.Managers
{
    internal class AntiAliasingManager : BooleanGraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            QualitySettings.antiAliasing = value;
        }
    }
}