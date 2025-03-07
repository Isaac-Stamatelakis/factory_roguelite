using UnityEngine.Device;

namespace UI.GraphicSettings.Managers
{
    internal class FullScreenManager : BooleanGraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            Screen.fullScreen = value != 0;
        }
    }
}