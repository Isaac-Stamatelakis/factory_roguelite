using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule.Tools {
    public interface IRobotTool {
        
        
    }
    public interface ILeftClickableTool {
        public void leftClick();
    }
    public interface IRightClickableTool {
        public void rightClick();
    }
    public abstract class RobotTool : ScriptableObject, IRobotTool
    {
        public Sprite sprite;
    }
}

