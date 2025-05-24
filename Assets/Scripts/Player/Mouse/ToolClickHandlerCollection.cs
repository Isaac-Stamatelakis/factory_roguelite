using System.Collections.Generic;
using Player.Tool;
using PlayerModule;
using Robot.Tool;
using UnityEngine;

namespace Player.Mouse
{
    public class ToolClickHandlerCollection
    {
        private HoldClickHandler recentlyUsed;
       
        private Dictionary<RobotToolType, HoldClickHandler> clickHandlerDict = new();

        public ToolClickHandlerCollection(PlayerRobot playerRobot)
        {
            for (var index = 0; index < playerRobot.ToolTypes.Count; index++)
            {
                var toolType = playerRobot.ToolTypes[index];
                var robotToolInstance = playerRobot.RobotTools[index];
                clickHandlerDict[toolType] = new HoldClickHandler(robotToolInstance);
            }
        }
        public HoldClickHandler GetOrAddTool(RobotToolType robotToolType, IRobotToolInstance toolInstance)
        {
            var handler = clickHandlerDict[robotToolType];
            if (!ReferenceEquals(handler, recentlyUsed))
            {
                recentlyUsed?.Terminate();
            }
            recentlyUsed = handler;
            return handler;
        }

        public void Terminate()
        {
            if (recentlyUsed == null) return;
            recentlyUsed.Terminate();
            recentlyUsed = null;
        }
    }
}