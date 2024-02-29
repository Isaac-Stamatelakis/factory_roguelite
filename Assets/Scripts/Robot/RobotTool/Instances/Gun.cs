using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Tools {
    [CreateAssetMenu(fileName = "New Gun", menuName = "Robots/Tools/Gun")]
    public class Gun : RobotTool, ILeftClickableTool
    {
        public void leftClick()
        {
            throw new System.NotImplementedException();
        }
    }
}

