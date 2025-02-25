using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using Item.Slot;
using Items;
using Items.Inventory;
using Player;
using Player.Tool;
using Robot;
using Robot.Tool;
using Robot.Upgrades;
using Robot.Upgrades.Network;
using RobotModule;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TileEntity.Instances.Robot.Upgrader
{
    public class RobotUpgraderUI : MonoBehaviour, ITileEntityUI<RobotUpgraderInstance>
    {
        [SerializeField] private AssetReference RobotUpgradeUPrefabI;
        [SerializeField] private TextMeshProUGUI mRobotName;
        [SerializeField] private ItemSlotUI mRobotItemSlotUI;
        [SerializeField] private Button mUpgradeButton;
        [SerializeField] private InventoryUI mToolInventoryUI;
        private List<UpgradeDisplayData> toolClickDataList;

        private RobotUpgradeUI robotUpgradeUIPrefab;
        public void Start()
        {
            StartCoroutine(LoadAssetsCoroutine());
        }

        private IEnumerator LoadAssetsCoroutine()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(RobotUpgradeUPrefabI);
            yield return handle;
            robotUpgradeUIPrefab = handle.Result.GetComponent<RobotUpgradeUI>();
        }

        public void DisplayTileEntityInstance(RobotUpgraderInstance tileEntityInstance)
        {
            PlayerRobot playerRobot = PlayerManager.Instance.GetPlayer().PlayerRobot;
            RobotItemData robotItemData = playerRobot.RobotData;
            RobotItem robotItem = playerRobot.robotItemSlot?.itemObject as RobotItem;
            DisplayRobot(robotItem, robotItemData, playerRobot.RobotTools);
        }

        public void DisplayRobot(RobotItem robotItem, RobotItemData robotItemData, List<IRobotToolInstance> toolInstances)
        {
            bool error = false;
            if (!robotItem)
            {
                Debug.LogWarning("Tried to display null robot item");
                error = true;
            }
            if (robotItemData == null)
            {
                Debug.LogWarning("Tried to display null robot data");
                error = true;
            }
            if (error)
            {
                Destroy(gameObject);
                return;
            }
            mRobotItemSlotUI.Display(new ItemSlot(robotItem,1,null));
            mRobotName.text = robotItem.name;
            mUpgradeButton.onClick.AddListener(() =>
            {
                RobotObject robotObject = robotItem.robot;
                string upgradePath = robotObject.UpgradePath;
                DisplayPath(upgradePath,robotItemData.RobotUpgrades);
            });

            
            List<ItemSlot> toolItems = RobotToolFactory.ToolInstancesToItems(toolInstances);
            mToolInventoryUI.DisplayInventory(toolItems);
            mToolInventoryUI.OverrideClickAction(OnToolClick);
            toolClickDataList = new List<UpgradeDisplayData>();
            robotItemData.ToolData.Upgrades ??= new List<List<RobotUpgradeData>>();
            for (var index = 0; index < toolInstances.Count; index++)
            {
                if (index >= robotItemData.ToolData.Upgrades.Count)
                {
                    robotItemData.ToolData.Upgrades.Add(new List<RobotUpgradeData>());
                }
                var upgradeData = robotItemData.ToolData.Upgrades[index];
                var tool = toolInstances[index];
                toolClickDataList.Add(new UpgradeDisplayData(tool.GetToolObject().UpgradePath, upgradeData));
            }
        }

        private void OnToolClick(int index)
        {
            DisplayPath(toolClickDataList[index]);
        }

        private void DisplayPath(UpgradeDisplayData upgradeDisplayData)
        {
            DisplayPath(upgradeDisplayData.UpgradePath,upgradeDisplayData.UpgradeData);
        }
        private void DisplayPath(string upgradePath, List<RobotUpgradeData> upgradeData)
        {
            SerializedRobotUpgradeNodeNetwork sNetwork = RobotUpgradeUtils.DeserializeRobotNodeNetwork(upgradePath);
            RobotUpgradeNodeNetwork upgradeNodeNetwork = RobotUpgradeUtils.FromSerializedNetwork(sNetwork, upgradeData);
            
            if (upgradeNodeNetwork == null) return;
            RobotUpgradeUI robotUpgradeUI = Instantiate(robotUpgradeUIPrefab);
            CanvasController.Instance.DisplayObject(robotUpgradeUI.gameObject);
            robotUpgradeUI.Initialize(upgradePath,upgradeNodeNetwork);
        }

        private struct UpgradeDisplayData
        {
            public string UpgradePath;
            public List<RobotUpgradeData> UpgradeData;

            public UpgradeDisplayData(string upgradePath, List<RobotUpgradeData> upgradeData)
            {
                UpgradePath = upgradePath;
                UpgradeData = upgradeData;
            }
        }
    }
}
