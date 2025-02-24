using System.Collections.Generic;
using Player.Tool;
using UnityEngine;

namespace Robot.Upgrades.LoadOut
{
    public class RobotUpgradeLoadOut
    {
        public RobotStatLoadOutCollection SelfLoadOuts;
        public Dictionary<RobotToolType, RobotStatLoadOutCollection> ToolLoadOuts;

        public RobotUpgradeLoadOut(RobotStatLoadOutCollection selfLoadOuts, Dictionary<RobotToolType, RobotStatLoadOutCollection> toolLoadOuts)
        {
            SelfLoadOuts = selfLoadOuts;
            ToolLoadOuts = toolLoadOuts;
        }
    }

    public class RobotStatLoadOutCollection
    {
        public int Current;
        public List<RobotStatLoadOut> LoadOuts;

        public RobotStatLoadOutCollection(int current, List<RobotStatLoadOut> loadOuts)
        {
            Current = current;
            LoadOuts = loadOuts;
        }

        public RobotStatLoadOut GetLoadOut(int index)
        {
            if (index < 0 || index >= LoadOuts.Count) return null;
            return LoadOuts[index];
        }

        public RobotStatLoadOut GetCurrent()
        {
            return GetLoadOut(Current);
        }
    }
    
}
