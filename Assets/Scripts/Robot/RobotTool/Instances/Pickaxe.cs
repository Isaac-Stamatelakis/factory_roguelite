using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Tools {
    [CreateAssetMenu(fileName = "New Pickaxe", menuName = "Robots/Tools/Pickaxe")]
    public class Pickaxe : RobotTool, ILeftClickableTool
    {
        public float speed;
        public Animator animator;
        public void leftClick()
        {
            
        }

    }
}

