using System.Collections.Generic;
using Player.Tool;
using UnityEngine;

namespace Robot.Tool.Object
{
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Robots/ToolCollection")]
    public class RobotToolObjectCollection : ScriptableObject
    {
        public List<RobotToolObject> Tools;
    }
}
