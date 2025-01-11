using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Drill", menuName = "Robots/Tools/Cutter")]
    public class RobotConduitCutterObject : RobotToolObject
    {
        public Sprite ItemLayerSprite;
        public Sprite FluidLayerSprite;
        public Sprite EnergyLayerSprite;
        public Sprite SignalLayerSprite;
        public Sprite MatrixLayerSprite;
        public LineRenderer LineRendererPrefab;
    }
}
