using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using RobotModule;
using Items.Tags;
using Robot;
using Robot.Tool.Object;

namespace Items {
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Item/Instances/RobotObject")]
    public class RobotItem : ItemObject, ITaggableItem
    {
        public GameStageObject GameStageObject;
        public RobotObject robot;
        
        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Stack;
        }

        public override GameStageObject GetGameStageObject()
        {
            return GameStageObject;
        }

        public override void SetGameStageObject(GameStageObject gameStageObject)
        {
            GameStageObject = gameStageObject;
        }
        public override Sprite GetSprite()
        {
            return robot.defaultSprite;
        }

        public override Sprite[] GetSprites()
        {
            return new Sprite[]{robot.defaultSprite};
        }

        public List<ItemTag> getTags()
        {
            return new List<ItemTag>{ItemTag.RobotData};
        }
    }
}
