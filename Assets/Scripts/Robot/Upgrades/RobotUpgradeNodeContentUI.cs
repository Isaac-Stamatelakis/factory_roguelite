using Items.Inventory;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Robot.Upgrades
{
    public class RobotUpgradeNodeContentUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mItemList;
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private TextMeshProUGUI mDescriptionText;
        [SerializeField] private Button mUpgradeButton;
        private RobotUpgradeNode robotUpgradeNode;
        public RobotUpgradeNode RobotUpgradeNode => robotUpgradeNode;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUIPrefab;
        [SerializeField] private UpgradeCostItemUI mUpgradeCostItemUIPrefab;
        
        public SerializedItemSlotEditorUI ItemSlotEditorUIPrefab => mItemSlotEditorUIPrefab;

        private RobotUpgradeNodeNetwork nodeNetwork;
        public void Initialize(RobotUpgradeNode robotUpgradeNode, RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.robotUpgradeNode = robotUpgradeNode;
            Display();
        }
        public void Display()
        {
            DisplayItemCost();
            mUpgradeButton.onClick.AddListener(OnUpgradeClick);
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type, nodeNetwork.SubType);
            mTitleText.text = robotUpgradeInfo?.GetTitle(robotUpgradeNode.NodeData.UpgradeType);
            mDescriptionText.text = robotUpgradeInfo?.GetDescription(robotUpgradeNode.NodeData.UpgradeType);
        }

        public void DisplayItemCost()
        {
            GlobalHelper.deleteAllChildren(mItemList.transform);
            for (var i = 0; i < robotUpgradeNode.NodeData.Cost.Count; i++)
            {
                var serializedItemSlot = robotUpgradeNode.NodeData.Cost[i];
                UpgradeCostItemUI upgradeCostItemUI = Instantiate(mUpgradeCostItemUIPrefab, mItemList.transform);
                upgradeCostItemUI.Initialize(serializedItemSlot,i,this);
            }
        }

        public void OnUpgradeClick()
        {
            Debug.Log("Update");
        }
    }
}
