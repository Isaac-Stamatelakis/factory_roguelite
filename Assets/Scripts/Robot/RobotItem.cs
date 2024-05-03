using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotModule;
using ItemModule.Tags;

namespace ItemModule {
    [CreateAssetMenu(fileName ="I~New Robot Item",menuName="Item/Instances/Robot")]
    public class RobotItem : ItemObject, ITaggable
    {
        public Robot robot;
        public override Sprite getSprite()
        {
            return robot.defaultSprite;
        }
    }
}
