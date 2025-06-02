using System;
using System.Collections.Generic;
using Player;
using Player.Controls;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using TMPro;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Indicators.General
{
    public class RobotLoadOutIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        private PlayerScript playerScript;
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
            SetLoadOut(playerScript.PlayerRobot.RobotUpgradeLoadOut.SelfLoadOuts.Current);
        }
        [SerializeField] private TextMeshProUGUI mTextElement;
        public void SetLoadOut(int loadOut)
        {
            mTextElement.text = RobotUpgradeUtils.FormatLoadOut(loadOut);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           ToolTipController.Instance.ShowToolTip(transform.position,"Modify Load Out");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OpenRobotLoadOut();
        }

        public void OpenRobotLoadOut()
        {
            PlayerRobot playerRobot = playerScript.PlayerRobot;
            string upgradePath = playerRobot.CurrentRobot.UpgradePath;
            List<RobotUpgradeData> upgradeData = playerRobot.RobotData.RobotUpgrades;
            RobotStatLoadOutCollection statLoadOutCollection = playerRobot.RobotUpgradeLoadOut.SelfLoadOuts;
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Robot,0);
                    
            void OnLoadOutChange(int loadOut)
            {
                SetLoadOut(loadOut);
            }

            Dictionary<int, Action> callbacks = new Dictionary<int, Action>
            {
                [(int)RobotUpgrade.Reach] = playerScript.OnReachUpgradeChange,
                [(int)RobotUpgrade.Flight] = playerRobot.OnFlightUpgradeChange,
                [(int)RobotUpgrade.RocketBoots] = playerRobot.OnRocketBootUpgradeChange,
                [(int)RobotUpgrade.TilePlacementRate] = playerScript.PlayerMouse.SyncTilePlacementCooldown
            };
            RobotUpgradeStatSelectorUI.UpgradeDisplayData upgradeDisplayData = new RobotUpgradeStatSelectorUI.UpgradeDisplayData(
                upgradePath, statLoadOutCollection, upgradeData, robotUpgradeInfo, OnLoadOutChange,callbacks,"Robot Upgrades");
            RobotUpgradeStatSelectorUI statSelectorUI = GameObject.Instantiate(playerScript.PlayerInventory.PlayerRobotToolUI.robotUpgradeStatSelectorUIPrefab);
            bool success = statSelectorUI.Display(upgradeDisplayData);
            if (success) CanvasController.Instance.DisplayObject(statSelectorUI.gameObject);
        }

        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.OpenRobotLoadOut;
        }
    }
}
