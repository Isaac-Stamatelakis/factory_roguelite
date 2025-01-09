using Robot.Tool;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Tool.UI
{
    public class PlayerToolListElementUI : MonoBehaviour
    {
        [SerializeField] private Image mImage;
        public void Display(RobotTool robotTool)
        {
            mImage.sprite = robotTool.GetSprite();
        }
    }
}
