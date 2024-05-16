using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotModule;
using Items.Tags;

namespace Items {
    [CreateAssetMenu(fileName ="I~New Robot Item",menuName="Item/Instances/Robot")]
    public class RobotItem : ItemObject, ITaggableItem
    {
        public Robot robot;

        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Stack;
        }

        public override Sprite getSprite()
        {
            return robot.defaultSprite;
        }

        public override Sprite[] getSprites()
        {
            return new Sprite[]{robot.defaultSprite};
        }

        public List<ItemTag> getTags()
        {
            return new List<ItemTag>{ItemTag.RobotData};
        }
    }
}
