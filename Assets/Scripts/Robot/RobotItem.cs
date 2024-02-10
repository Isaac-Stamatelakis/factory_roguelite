using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotModule;

namespace ItemModule {
    [CreateAssetMenu(fileName ="I~New Robot Item",menuName="Item Register/Robot")]
    public class RobotItem : ItemObject
    {
        public Robot robot;
        public override Sprite getSprite()
        {
            return robot.defaultSprite;
        }
    }
}
