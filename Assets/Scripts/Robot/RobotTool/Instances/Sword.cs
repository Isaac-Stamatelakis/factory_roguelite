using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Tools {
    [CreateAssetMenu(fileName = "New Sword", menuName = "Robots/Tools/Sword")]
    public class Sword : RobotTool, ILeftClickableTool
    {
        public float damage;
        public Animator animator;
        public void leftClick()
        {
            
        }
    }
}

