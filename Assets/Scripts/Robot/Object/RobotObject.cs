using RobotModule;
using UnityEngine;

namespace Robot {
    /// <summary>
    /// Represents a robot which the player controls
    /// Robots have movement, tools, and an inventory
    /// Robots are also able to be put into robot team controllers which can automate resource gathering
    /// </summary>
    public abstract class RobotObject : ScriptableObject, IRobot
    {
        public Sprite defaultSprite;
        public abstract void handleMovement(Transform playerTransform);
        public abstract void init(GameObject playerGameObject);
    }

}

