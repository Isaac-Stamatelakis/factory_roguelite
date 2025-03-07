using UnityEngine;

namespace UI.GraphicSettings.Managers
{
    internal class ViewRangeGraphicManager : GraphicSettingManager, IScrollBarGraphicManager
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

        public int GetSteps()
        {
            return System.Enum.GetValues(typeof(CameraViewSize)).Length;
        }
    }
}