using Player.UI.Inventory;
using UI.PlayerInvUI;
using UnityEngine;

namespace Player.UI
{
    public class PlayerInventoryUI : MonoBehaviour
    {
        [SerializeField] private StackedPlayerInvUIElement stackedPlayerInvUIElement;
        [SerializeField] private PlayerInventoryRobotInfo robotInfo;

        public void Display(PlayerScript playerScript)
        {
            robotInfo.Display(playerScript.PlayerRobot);
        }
    }
}
