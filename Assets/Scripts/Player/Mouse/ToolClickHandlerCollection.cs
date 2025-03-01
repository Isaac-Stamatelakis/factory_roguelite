using System.Collections.Generic;
using Player.Tool;
using Robot.Tool;

namespace Player.Mouse
{
    public class ToolClickHandlerCollection
    {
        private Dictionary<MouseButtonKey, HoldClickHandler> recentlyUsed =
            new Dictionary<MouseButtonKey, HoldClickHandler>();
       
        private Dictionary<RobotToolType, Dictionary<MouseButtonKey, HoldClickHandler>> clickHandlerDict =
            new Dictionary<RobotToolType, Dictionary<MouseButtonKey, HoldClickHandler>>();
        public HoldClickHandler GetOrAddTool(RobotToolType robotToolType, MouseButtonKey mouseButtonKey, IRobotToolInstance toolInstance)
        {
            if (!clickHandlerDict.ContainsKey(robotToolType))
            {
                clickHandlerDict.Add(robotToolType, new Dictionary<MouseButtonKey, HoldClickHandler>());
            }

            if (!clickHandlerDict[robotToolType].ContainsKey(mouseButtonKey))
            {
                clickHandlerDict[robotToolType][mouseButtonKey] = new HoldClickHandler(toolInstance,mouseButtonKey) ;
            
            }
            var handler = clickHandlerDict[robotToolType][mouseButtonKey];
            var last = recentlyUsed.GetValueOrDefault(mouseButtonKey);
            if (!ReferenceEquals(handler, last))
            {
                last?.Terminate();
            }
            recentlyUsed[mouseButtonKey] = handler;
            return handler;
        }

        public void Terminate(MouseButtonKey mouseButtonKey)
        {
            if (!recentlyUsed.ContainsKey(mouseButtonKey)) return;
            recentlyUsed[mouseButtonKey].Terminate();
            recentlyUsed.Remove(mouseButtonKey);
        }

        public ToolClickHandlerCollection()
        {
        }
    }
}