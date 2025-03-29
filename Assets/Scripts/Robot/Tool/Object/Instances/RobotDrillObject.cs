using Robot.Tool.Instances.Drill;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Drill", menuName = "Robots/Tools/Drill")]
    public class RobotDrillObject : RobotToolObject
    {
        public Sprite BaseLayerSprite;
        public Sprite BackgroundLayerSprite;
        public Sprite FluidLayerSprite;
        public LineRenderer LineRendererPrefab;
        public ParticleSystem ParticleEmitterPrefab;
        public LaserDrillAudioController AudioControllerPrefab;
    }
}
