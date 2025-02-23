using System.Collections.Generic;
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
        private RobotUpgradeNode robotUpgradeNode;
        public RobotUpgradeNode RobotUpgradeNode => robotUpgradeNode;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUIPrefab;
        [SerializeField] private UpgradeCostItemUI mUpgradeCostItemUIPrefab;
        
        public SerializedItemSlotEditorUI ItemSlotEditorUIPrefab => mItemSlotEditorUIPrefab;

        private RobotUpgradeNodeNetwork nodeNetwork;
        public bool UnActivated => robotUpgradeNode == null;

        public void Initialize(RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.nodeNetwork = nodeNetwork;
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type, nodeNetwork.SubType);
            mTitleDropDown.options = robotUpgradeInfo?.GetDropDownOptions() ?? new List<TMP_Dropdown.OptionData>();
            mUpgradeButton.onClick.AddListener(OnUpgradeClick);
            
        }
        public void DisplayUpgradeNode(RobotUpgradeNode robotUpgradeNode)
        {
            this.robotUpgradeNode = robotUpgradeNode;
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type, nodeNetwork.SubType);

            bool editable = SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME;
            
            mTitleDropDown.onValueChanged.RemoveAllListeners();
            mAddItemButton.onClick.RemoveAllListeners();
            
            mTitleDropDown.value = robotUpgradeNode.NodeData.UpgradeType;
            mTitleDropDown.interactable = editable;
            mAddItemButton.gameObject.SetActive(editable);
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
            }
            
            
            mDescriptionText.text = robotUpgradeInfo?.GetDescription(robotUpgradeNode.NodeData.UpgradeType);
            
            DisplayItemCost();
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
