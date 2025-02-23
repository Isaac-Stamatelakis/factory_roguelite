using Items;
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
        public ItemObject ToolIconItem;
        [Header("From StreamingAssets/Upgrade")]
        public string UpgradePath;
    }
}
