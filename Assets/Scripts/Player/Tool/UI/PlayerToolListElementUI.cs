using UnityEngine;
using UnityEngine.UI;

namespace Player.Tool.UI
{
    public class PlayerToolListElementUI : MonoBehaviour
    {
        [SerializeField] private Image mImage;
        public void Display(PlayerTool playerTool)
        {
            mImage.sprite = playerTool.GetSprite();
        }
    }
}
