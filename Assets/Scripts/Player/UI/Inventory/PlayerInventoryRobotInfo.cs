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
using Robot.Upgrades.LoadOut;
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
                    playerRobot.RobotUpgradeLoadOut.SelfLoadOuts,
                    playerRobot.RobotData.RobotUpgrades,
                    RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Robot,0)
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
                RobotStatLoadOutCollection statLoadOutCollection = playerRobot.RobotUpgradeLoadOut.GetToolLoadOut(toolType);
                RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Tool,(int)toolType);
                UpgradeDisplayData upgradeDisplayData = new UpgradeDisplayData(upgradePath, statLoadOutCollection, upgradeData, robotUpgradeInfo);
                DisplayData(upgradeDisplayData);
            });
        }

        private void DisplayData(UpgradeDisplayData upgradeDisplayData)
        {
            bool error = false;
            if (upgradeDisplayData.StatLoadOutCollection == null)
            {
                error = true;
                Debug.LogWarning("Tried to display null stat load out");
            }

            SerializedRobotUpgradeNodeNetwork network = RobotUpgradeUtils.DeserializeRobotNodeNetwork(upgradeDisplayData.UpgradePath);
            if (network == null)
            {
                error = true;
                Debug.LogWarning("Tried to display null network");
            }

            if (error) return;
            bool prefabNotLoadedYet = !statSelectorUIPrefab;
            if (prefabNotLoadedYet) return;
            
            Dictionary<int, int> upgradeDict = RobotUpgradeUtils.GetAmountOfUpgrades(network.NodeData, upgradeDisplayData.UpgradeData);
            RobotUpgradeStatSelectorUI statSelectorUI = GameObject.Instantiate(statSelectorUIPrefab);
            statSelectorUI.Display(upgradeDisplayData.StatLoadOutCollection,upgradeDict,upgradeDisplayData.RobotUpgradeInfo);
            CanvasController.Instance.DisplayObject(statSelectorUI.gameObject);
        }
        

        private struct UpgradeDisplayData
        {
            public string UpgradePath;
            public RobotStatLoadOutCollection StatLoadOutCollection;
            public List<RobotUpgradeData> UpgradeData;
            public RobotUpgradeInfo RobotUpgradeInfo;

            public UpgradeDisplayData(string upgradePath, RobotStatLoadOutCollection statLoadOutCollection, List<RobotUpgradeData> upgradeData, RobotUpgradeInfo robotUpgradeInfo)
            {
                UpgradePath = upgradePath;
                StatLoadOutCollection = statLoadOutCollection;
                UpgradeData = upgradeData;
                RobotUpgradeInfo = robotUpgradeInfo;
            }
        }
    }
}
