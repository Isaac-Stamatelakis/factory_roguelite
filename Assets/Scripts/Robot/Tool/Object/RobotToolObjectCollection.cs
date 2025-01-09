using System.Collections.Generic;
using Player.Tool;
using UnityEngine;

namespace Robot.Tool.Object
{
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Robot/Tools/Collection")]
    public class RobotToolObjectCollection : ScriptableObject
    {
        public List<RobotToolObject> Tools;
    }
}
