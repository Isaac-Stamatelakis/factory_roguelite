using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using Player.Tool;
using Robot.Tool;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.Network;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace Player.UI.Inventory
{
    public class PlayerInventoryRobotInfo : MonoBehaviour
    {
        [SerializeField] private AssetReference mStatSelectorUIPrefabRef;
        [SerializeField] private ItemSlotUI mRobotItemSlotUI;
        [SerializeField] private Button mModifyStatButton;
        [SerializeField] private InventoryUI mToolInventoryUI;
        private RobotUpgradeStatSelectorUI statSelectorUIPrefab;

        public void Start()
        {
            StartCoroutine(LoadAssets());
        }

        private IEnumerator LoadAssets()
        {
            var statHandle = Addressables.LoadAssetAsync<GameObject>(mStatSelectorUIPrefabRef);
            yield return statHandle;
            statSelectorUIPrefab = statHandle.Result.GetComponent<RobotUpgradeStatSelectorUI>();
            Addressables.Release(statHandle);
        }

        public void Display(PlayerRobot playerRobot)
        {
            mRobotItemSlotUI.Display(playerRobot.robotItemSlot);
            mModifyStatButton.onClick.AddListener(() =>
            {
                UpgradeDisplayData upgradeDisplayData = new UpgradeDisplayData(
                    playerRobot.CurrentRobot?.UpgradePath,
                    playerRobot.RobotUpgradeLoadOut.SelfLoadOuts.GetCurrent(),
                    playerRobot.RobotData.RobotUpgrades
                );
                DisplayData(upgradeDisplayData);

            });
            List<ItemSlot> toolItems = RobotToolFactory.ToolInstancesToItems(playerRobot.RobotTools);
            mToolInventoryUI.DisplayInventory(toolItems);
            mToolInventoryUI.OverrideClickAction((int index) =>
            {
                string upgradePath = playerRobot.RobotTools[index].GetToolObject().UpgradePath;
                List<RobotUpgradeData> upgradeData = playerRobot.RobotData.ToolData.Upgrades[index];
                RobotToolType toolType = playerRobot.ToolTypes[index];
                RobotStatLoadOut statLoadOut = playerRobot.RobotUpgradeLoadOut.GetToolLoadOut(toolType)?.GetCurrent();
                UpgradeDisplayData upgradeDisplayData = new UpgradeDisplayData(upgradePath, statLoadOut, upgradeData);
                DisplayData(upgradeDisplayData);
            });
        }

        private void DisplayData(UpgradeDisplayData upgradeDisplayData)
        {
            if (upgradeDisplayData.StatLoadOut == null) return;
            SerializedRobotUpgradeNodeNetwork network = RobotUpgradeUtils.DeserializeRobotNodeNetwork(upgradeDisplayData.UpgradePath);
            if (network == null) return;
            Dictionary<int, int> upgradeDict = RobotUpgradeUtils.BuildUpgradeDict(network.NodeData, upgradeDisplayData.UpgradeData);
            RobotUpgradeStatSelectorUI statSelectorUI = GameObject.Instantiate(statSelectorUIPrefab);
            statSelectorUI.Display(upgradeDisplayData.StatLoadOut,upgradeDict,RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Robot,0));
            CanvasController.Instance.DisplayObject(statSelectorUI.gameObject);
        }
        

        private struct UpgradeDisplayData
        {
            public string UpgradePath;
            public RobotStatLoadOut StatLoadOut;
            public List<RobotUpgradeData> UpgradeData;

            public UpgradeDisplayData(string upgradePath, RobotStatLoadOut statLoadOut, List<RobotUpgradeData> upgradeData)
            {
                UpgradePath = upgradePath;
                StatLoadOut = statLoadOut;
                UpgradeData = upgradeData;
            }
        }
    }
}
