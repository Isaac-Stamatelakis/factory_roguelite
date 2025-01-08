using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Drill", menuName = "Player/Tools/Drill")]
    public class PlayerDrillObject : PlayerToolObject
    {
        public Sprite BaseLayerSprite;
        public Sprite BackgroundLayerSprite;
        public LineRenderer LineRendererPrefab;
    }
}
