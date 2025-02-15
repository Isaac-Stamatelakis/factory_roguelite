using UnityEngine;

namespace Player.Tool
{
    public enum RobotToolType
    {
        LaserGun,
        LaserDrill,
        ConduitSlicers,
        Buildinator
    }
    
    public abstract class RobotToolObject : ScriptableObject
    {
        public Sprite ToolSprite;
    }
}
