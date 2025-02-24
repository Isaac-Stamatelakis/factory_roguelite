using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Robot.Upgrades;
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
                
                SerializedRobotUpgradeNodeNetwork network = RobotUpgradeUtils.DeserializeRobotNodeNetwork(playerRobot.CurrentRobot?.UpgradePath);
                if (network == null) return;
                Dictionary<int, int> upgradeDict = RobotUpgradeUtils.BuildUpgradeDict(network.NodeData, playerRobot.RobotData.RobotUpgrades);
                RobotUpgradeStatSelectorUI statSelectorUI = GameObject.Instantiate(statSelectorUIPrefab);
                statSelectorUI.Display(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts.GetCurrent(),upgradeDict,RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Robot,0));
                CanvasController.Instance.DisplayObject(statSelectorUI.gameObject);
            });
        }
    }
}
