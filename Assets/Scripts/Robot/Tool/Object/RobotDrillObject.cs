using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Drill", menuName = "RobotObject/Tools/Drill")]
    public class RobotDrillObject : RobotToolObject
    {
        public Sprite BaseLayerSprite;
        public Sprite BackgroundLayerSprite;
        public LineRenderer LineRendererPrefab;
    }
}
