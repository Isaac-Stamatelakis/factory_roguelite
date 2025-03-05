using System;
using System.Collections.Generic;
using System.Globalization;
using DevTools;
using Item.Slot;
using Items;
using Items.Inventory;
using Player;
using PlayerModule;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Robot.Upgrades
{
    public class RobotUpgradeNodeContentUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mItemList;
        [SerializeField] private TMP_Dropdown mTitleDropDown;
        [SerializeField] private TextMeshProUGUI mDescriptionText;
        [SerializeField] private Button mUpgradeButton;
        [SerializeField] private Button mAddItemButton;
        [SerializeField] private Transform mEditElementContainer;
        [SerializeField] private TMP_InputField mCostMultiplerField;
        [SerializeField] private TMP_InputField mAmountField;
        [SerializeField] private ItemSlotUI mItemSlotUI;
        private RobotUpgradeNode robotUpgradeNode;
        public RobotUpgradeNode RobotUpgradeNode => robotUpgradeNode;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUIPrefab;
        [SerializeField] private UpgradeCostItemUI mUpgradeCostItemUIPrefab;
        [SerializeField] private TextMeshProUGUI mProgressText;
        public SerializedItemSlotEditorUI ItemSlotEditorUIPrefab => mItemSlotEditorUIPrefab;

        private RobotUpgradeNodeNetwork nodeNetwork;
        private RobotUpgradeNetworkUI networkUI;
        public bool UnActivated => robotUpgradeNode == null;

        public void Initialize(RobotUpgradeNetworkUI networkUI, RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.networkUI = networkUI;
            this.nodeNetwork = nodeNetwork;
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type, nodeNetwork.SubType);
            mTitleDropDown.options = robotUpgradeInfo?.GetDropDownOptions() ?? new List<TMP_Dropdown.OptionData>();
            mUpgradeButton.onClick.AddListener(OnUpgradeClick);
        }
        
        public void DisableEditElements()
        {
            mEditElementContainer.gameObject.SetActive(false);
        }
        public void DisplayUpgradeNode(RobotUpgradeNode robotUpgradeNode)
        {
            this.robotUpgradeNode = robotUpgradeNode;
            if (this.robotUpgradeNode == null) return;
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type, nodeNetwork.SubType);

            bool editable = SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME;
            
            mTitleDropDown.onValueChanged.RemoveAllListeners();
            mAddItemButton.onClick.RemoveAllListeners();
            mAmountField.onValueChanged.RemoveAllListeners();
            mItemSlotUI.GetComponent<Button>().onClick.RemoveAllListeners();
            mCostMultiplerField.onValueChanged.RemoveAllListeners();
            
            mTitleDropDown.value = robotUpgradeNode.NodeData.UpgradeType;
            mTitleDropDown.interactable = editable;
            mAddItemButton.gameObject.SetActive(editable);
            mEditElementContainer.gameObject.SetActive(editable);

            mUpgradeButton.interactable = UpgradeButtonInteractable();
            DisplayItemIcon();
            
            if (editable)
            {
                mTitleDropDown.GetComponent<Image>().enabled = true;
                Image[] images = mTitleDropDown.GetComponentsInChildren<Image>();
                foreach (Image image in images)
                {
                    image.gameObject.SetActive(true);
                }
                mTitleDropDown.onValueChanged.AddListener((value) =>
                {
                    robotUpgradeNode.NodeData.UpgradeType = value;
                    mDescriptionText.text = robotUpgradeInfo?.GetDescription(robotUpgradeNode.NodeData.UpgradeType);
                });
                mAddItemButton.onClick.AddListener(() =>
                {
                    robotUpgradeNode.NodeData.Cost.Add(new SerializedItemSlot("stone",1,null));
                    DisplayItemCost();
                });
                
                mCostMultiplerField.text = this.robotUpgradeNode.NodeData.CostMultiplier.ToString(CultureInfo.InvariantCulture);
                mAmountField.text = this.robotUpgradeNode.NodeData.UpgradeAmount.ToString();
               
                mAmountField.onValueChanged.AddListener((value) =>
                {
                    try
                    {
                        int amount = Convert.ToInt32(value);
                        if (amount < 0) amount = 1;
                        robotUpgradeNode.NodeData.UpgradeAmount = amount;
                        DisplayItemIcon();
                    }
                    catch (FormatException)
                    {
                        mAmountField.text = robotUpgradeNode.NodeData.UpgradeAmount.ToString();
                    }
                });
                mCostMultiplerField.onValueChanged.AddListener((value) =>
                {
                    try
                    {
                        float multiplier = Convert.ToSingle(value);
                        if (multiplier < 1) multiplier = 1;
                        robotUpgradeNode.NodeData.CostMultiplier = multiplier;
                    }
                    catch (FormatException)
                    {
                        mCostMultiplerField.text = robotUpgradeNode.NodeData.CostMultiplier.ToString(CultureInfo.InvariantCulture);
                    }
                });
                mItemSlotUI.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SerializedItemSlotEditorUI itemSlotEditorUI = Instantiate(ItemSlotEditorUIPrefab);
                    List<SerializedItemSlot> serializedItemSlots = new List<SerializedItemSlot> { new(robotUpgradeNode.NodeData.IconItemId,1,null) };
                    itemSlotEditorUI.Init(serializedItemSlots,0,null,gameObject,displayTags:false,displayAmount:false,displayTrash:false,displayArrows:false,callback:OnIconChange);
                    itemSlotEditorUI.transform.SetParent(transform.parent,false);
                });
            }
            
            
            mDescriptionText.text = robotUpgradeInfo?.GetDescription(robotUpgradeNode.NodeData.UpgradeType);
            
            DisplayItemCost();
        }

        private bool UpgradeButtonInteractable()
        {
            if (this.robotUpgradeNode.IsCompleted()) return false;
            foreach (int id in robotUpgradeNode.GetPrerequisites())
            {
                foreach (var node in nodeNetwork.GetNodes())
                {
                    if (node.GetId() != id) continue;
                    if (!node.IsCompleted()) return false;
                }
            }

            return true;
        }
        

        private void DisplayItemIcon()
        {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(robotUpgradeNode.NodeData.IconItemId);
            if (!itemObject)
            {
                itemObject = ItemRegistry.GetInstance().GetItemObject("stone");
            }
            ItemSlot itemSlot = new ItemSlot(itemObject, 1, null);
            int upgradedAmount = robotUpgradeNode.InstanceData?.Amount ?? 0;
            mProgressText.text = $"Upgraded {upgradedAmount}/{robotUpgradeNode.NodeData.UpgradeAmount}";
            mProgressText.color = upgradedAmount >= robotUpgradeNode.NodeData.UpgradeAmount ? Color.green : Color.white;
            mItemSlotUI.Display(itemSlot);
        }
        

        private void OnIconChange(SerializedItemSlot serializedItemSlot)
        {
            robotUpgradeNode.NodeData.IconItemId = serializedItemSlot.id;
            networkUI.Display();
            DisplayItemIcon();
        }

        public void DisplayItemCost()
        {
            GlobalHelper.deleteAllChildren(mItemList.transform);
            for (var i = 0; i < robotUpgradeNode.NodeData.Cost.Count; i++)
            {
                UpgradeCostItemUI upgradeCostItemUI = Instantiate(mUpgradeCostItemUIPrefab, mItemList.transform);
                upgradeCostItemUI.Initialize(robotUpgradeNode,i,this);
            }
        }

        private void IterateUpgradeAmount(int amount = 1)
        {
            robotUpgradeNode.InstanceData.Amount += amount;
            networkUI.Display();
            DisplayItemIcon();
            DisplayItemCost();
        }

        public void OnUpgradeClick()
        {
            bool editable = SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME;
            robotUpgradeNode.InstanceData ??= new RobotUpgradeData(robotUpgradeNode.GetId(), 0);
            if (robotUpgradeNode.InstanceData.Amount >= robotUpgradeNode.NodeData.UpgradeAmount) return;
            if (editable)
            {
                int amount = Input.GetKeyDown(KeyCode.LeftShift) ? -1 : 1; // For testing
                IterateUpgradeAmount(amount);
                return;
            }

            PlayerManager playerManager = PlayerManager.Instance;
            if (!playerManager) return;
            PlayerScript playerScript = playerManager.GetPlayer();
            PlayerInventory playerInventory = playerScript.PlayerInventory;

            uint costMultiplier = RobotUpgradeUtils.GetRequireAmountMultiplier(robotUpgradeNode);
            foreach (SerializedItemSlot serializedItemSlot in robotUpgradeNode.NodeData.Cost)
            {
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(serializedItemSlot);
                itemSlot.amount *= costMultiplier;
                uint playerAmount = ItemSlotUtils.AmountOf(itemSlot, playerInventory.Inventory);
                if (playerAmount < itemSlot.amount) return;
            }
            
            foreach (SerializedItemSlot serializedItemSlot in robotUpgradeNode.NodeData.Cost)
            {
                string id = serializedItemSlot.id;
                uint amount = serializedItemSlot.amount * costMultiplier;

                foreach (ItemSlot itemSlot in playerInventory.Inventory)
                {
                    if (ItemSlotUtils.IsItemSlotNull(itemSlot) || itemSlot.itemObject.id != id) continue;
                    if (itemSlot.amount >= amount)
                    {
                        itemSlot.amount -= amount;
                        break;
                    }
                    amount -= itemSlot.amount;
                    itemSlot.amount = 0;
                    
                    if (amount == 0) break;
                }
            }
            RobotUpgradeInfo upgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type,nodeNetwork.SubType);
            List<int> constUpgrades = upgradeInfo.GetConstantUpgrades();
            int upgrade = robotUpgradeNode.NodeData.UpgradeType;
            if (constUpgrades.Contains(upgrade))
            {
                UpdateConstantValues(playerScript.PlayerRobot,upgrade);
            }
            
            IterateUpgradeAmount();
            networkUI.Display();
            DisplayItemIcon();
        }

        private void UpdateConstantValues(PlayerRobot playerRobot, int upgrade)
        {
            int totalUpgrades = 0;
            foreach (var node in nodeNetwork.UpgradeNodes)
            {
                if (node.NodeData.UpgradeType != upgrade) continue;
                totalUpgrades += node.InstanceData.Amount;
            }
            
            RobotStatLoadOutCollection loadOutCollection = playerRobot.RobotUpgradeLoadOut.GetCollection(nodeNetwork.Type, nodeNetwork.SubType);
            foreach (RobotStatLoadOut robotStatLoadOut in loadOutCollection.LoadOuts)
            {
                if (robotStatLoadOut.DiscreteValues.ContainsKey(upgrade))
                {
                    robotStatLoadOut.DiscreteValues[upgrade] = totalUpgrades;
                } else if (robotStatLoadOut.ContinuousValues.ContainsKey(upgrade))
                {
                    robotStatLoadOut.DiscreteValues[upgrade] = totalUpgrades;
                }
            }

        }
    }
}
