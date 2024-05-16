using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tools {
    public interface IRobotTool {
        public void rightClick() {

        }
        public void leftClick() {
            
        }
    }
    public abstract class RobotToolItem : PresetItemObject, IRobotTool
    {
        
    }

}
