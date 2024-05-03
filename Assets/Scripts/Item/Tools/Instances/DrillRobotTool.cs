using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Tools {
    [CreateAssetMenu(fileName ="I~New Drill",menuName="Item/Instances/Tool/Drill")]
    public class DrillRobotTool : RobotToolItem
    {
        [SerializeField] private Sprite sprite;
        public override Sprite getSprite()
        {
            return sprite;
        }
    }
}

