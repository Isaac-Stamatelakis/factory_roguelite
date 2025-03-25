using Robot;
using RobotModule;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Robot
{
    public class PlayerRobotUI : MonoBehaviour
    {
        [SerializeField] private Scrollbar mHealthBar;
        [SerializeField] private Scrollbar mEnergyBar;
        
        public void Display(PlayerRobot playerRobot)
        {
            if (!playerRobot) return;
            RobotItemData robotData = playerRobot.RobotData;
            if (robotData == null) return;
            mHealthBar.size = robotData.Health / playerRobot.GetHealth();
            mEnergyBar.size = ((float)robotData.Energy) / playerRobot.GetEnergy();
        }
        
    }
}
