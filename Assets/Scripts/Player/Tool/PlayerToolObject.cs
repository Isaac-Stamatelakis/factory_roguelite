using UnityEngine;

namespace Player.Tool
{
    public enum PlayerToolType
    {
        LaserGun,
        LaserDrill,
        ConduitSlicers,
        Buildinator
    }
    [CreateAssetMenu(fileName = "New Player Tool", menuName = "Robots/Tool")]
    public class PlayerToolObject : ScriptableObject
    {
        public PlayerToolType ToolType;
        public Sprite[] Sprites;
    }
}
