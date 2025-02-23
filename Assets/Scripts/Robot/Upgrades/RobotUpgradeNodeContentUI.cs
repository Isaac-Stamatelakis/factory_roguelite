using System;
using System.Collections.Generic;
using System.Globalization;
using DevTools;
using Items.Inventory;
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
        [SerializeField] private Button mEditImageButton;
        [SerializeField] private TMP_InputField mCostMultiplerField;
        [SerializeField] private TMP_InputField mAmountField;
        private RobotUpgradeNode robotUpgradeNode;
        public RobotUpgradeNode RobotUpgradeNode => robotUpgradeNode;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUIPrefab;
        [SerializeField] private UpgradeCostItemUI mUpgradeCostItemUIPrefab;
        
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
            mEditImageButton.onClick.RemoveAllListeners();
            mCostMultiplerField.onValueChanged.RemoveAllListeners();
            
            mTitleDropDown.value = robotUpgradeNode.NodeData.UpgradeType;
            mTitleDropDown.interactable = editable;
            mAddItemButton.gameObject.SetActive(editable);
            mEditElementContainer.gameObject.SetActive(editable);
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
                
                mEditImageButton.onClick.AddListener(() =>
                {
                    SerializedItemSlotEditorUI itemSlotEditorUI = Instantiate(ItemSlotEditorUIPrefab);
                    List<SerializedItemSlot> serializedItemSlots = new List<SerializedItemSlot> { new(robotUpgradeNode.NodeData.IconItemId,1,null) };
                    itemSlotEditorUI.Init(serializedItemSlots,0,null,gameObject,displayTags:false,displayAmount:false,displayTrash:false,displayArrows:false,callback:OnIconChange);
                    itemSlotEditorUI.transform.SetParent(transform.parent,false);
                });
                mAmountField.onValueChanged.AddListener((value) =>
                {
                    try
                    {
                        int amount = Convert.ToInt32(value);
                        if (amount < 0) amount = 1;
                        robotUpgradeNode.NodeData.UpgradeAmount = amount;
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
            }
            
            
            mDescriptionText.text = robotUpgradeInfo?.GetDescription(robotUpgradeNode.NodeData.UpgradeType);
            
            DisplayItemCost();
        }

        private void OnIconChange(SerializedItemSlot serializedItemSlot)
        {
            robotUpgradeNode.NodeData.IconItemId = serializedItemSlot.id;
            networkUI.Display();
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

        public void OnUpgradeClick()
        {
            bool editable = SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME;
            if (editable) return;
        }
    }
}
