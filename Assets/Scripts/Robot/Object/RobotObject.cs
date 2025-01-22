using Robot.Tool.Object;
using RobotModule;
using UnityEngine;
using UnityEngine.Serialization;

namespace Robot {
    /// <summary>
    /// Represents a robot which the player controls
    /// Robots have movement, tools, and an inventory
    /// Robots are also able to be put into robot team controllers which can automate resource gathering
    /// </summary>
    public abstract class RobotObject : ScriptableObject, IRobot
    {
        [FormerlySerializedAs("Tools")] public RobotToolObjectCollection ToolCollection;
        public Sprite defaultSprite;
        public float BaseHealth;
        public ulong MaxEnergy;
        public abstract void handleMovement(Transform playerTransform);
        public abstract void init(GameObject playerGameObject);
    }

    public interface IEnergyRechargeRobot
    {
        ulong EnergyRechargeRate { get;}
    }

}

