using Robot;
using RobotModule;
using UI.GeneralUIElements.Sliders;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Robot
{
    public class PlayerRobotUI : MonoBehaviour
    {
        [SerializeField] private Color healthColor;
        [SerializeField] private Color energyColor;
        [SerializeField] private GridMaskedSliderUI mHealthBar;
        [SerializeField] private GridMaskedSliderUI mEnergyBar;

        public void Initialize()
        {
            const int SIZE = 10;
            Vector2Int cellSize = new Vector2Int(30, 40);

            mHealthBar.Initialize(healthColor, SIZE, cellSize);
            mEnergyBar.Initialize(energyColor, SIZE, cellSize);
        }
        public void Display(PlayerRobot playerRobot)
        {
            if (!playerRobot) return;
            RobotItemData robotData = playerRobot.RobotData;
            if (robotData == null) return;
            mHealthBar.SetFill(robotData.Health / playerRobot.GetMaxHealth());
            mEnergyBar.SetFill((float)robotData.Energy / playerRobot.GetEnergyStorage());
        }
        
    }
}
