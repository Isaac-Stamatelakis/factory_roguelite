using System;
using System.Collections;
using Item.Slot;
using Items;
using Items.Inventory;
using Robot;
using Robot.Upgrades;
using RobotModule;
using TMPro;
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
            
        }

        public void DisplayRobot(RobotItem robotItem, RobotItemData robotItemData)
        {
            mRobotItemSlotUI.Display(new ItemSlot(robotItem,1,null));
            mRobotName.text = robotItem.name;
            mUpgradeButton.onClick.AddListener(() =>
            {
                RobotUpgradeUI robotUpgradeUI = Instantiate(robotUpgradeUIPrefab);
                RobotObject robotObject = robotItem.robot;
                string upgradePath = robotObject.UpgradePath;
                
            });
        }
    }
}
