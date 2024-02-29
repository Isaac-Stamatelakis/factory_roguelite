using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Tools {
    [CreateAssetMenu(fileName = "New Teleporter", menuName = "Robots/Tools/Teleporter")]
    public class TeleportRod : RobotTool, ILeftClickableTool
    {
        public Animator animator;
        public void leftClick()
        {
            
        }
    }
}

