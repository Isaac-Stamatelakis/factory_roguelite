using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Buildinator", menuName = "Robots/Tools/Buildinator")]
    public class BuildinatorObject : RobotToolObject
    {
        public Sprite ChiselSprite;
        public Sprite RotatorSprite;
        public Sprite HammerSprite;
    }
}
