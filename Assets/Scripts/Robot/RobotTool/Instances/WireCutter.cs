using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Tools {
    [CreateAssetMenu(fileName = "New WireCutter", menuName = "Robots/Tools/WireCutter")]
    public class WireCutter : RobotTool, ILeftClickableTool
    {
        public void leftClick()
        {
            throw new System.NotImplementedException();
        }
    }
}

