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
        
        public void Display(RobotItemData robotData, RobotObject robot)
        {
            if (robotData == null || ReferenceEquals(robot, null)) return;
            mHealthBar.size = robotData.Health / robot.BaseHealth;
            mEnergyBar.size = ((float)robotData.Energy) / robot.MaxEnergy;
        }
        
    }
}
