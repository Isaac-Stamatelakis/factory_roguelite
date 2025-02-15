using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Cutter", menuName = "Robots/Tools/Cutter")]
    public class RobotConduitCutterObject : RobotToolObject
    {
        public Sprite ItemLayerSprite;
        public Sprite FluidLayerSprite;
        public Sprite EnergyLayerSprite;
        public Sprite SignalLayerSprite;
        public Sprite MatrixLayerSprite;
        public Sprite LaserModeSprite;
        public Sprite SpliceModeSprite;
        public LineRenderer LineRendererPrefab;
    }
}
