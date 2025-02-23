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
        private RobotUpgradeNode robotUpgradeNode;
        public RobotUpgradeNode RobotUpgradeNode => robotUpgradeNode;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUIPrefab;
        [SerializeField] private UpgradeCostItemUI mUpgradeCostItemUIPrefab;
        
        public SerializedItemSlotEditorUI ItemSlotEditorUIPrefab => mItemSlotEditorUIPrefab;

        private RobotUpgradeNodeNetwork nodeNetwork;
        public bool UnActivated => robotUpgradeNode == null;
        public void DisplayUpgradeNode(RobotUpgradeNode robotUpgradeNode, RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.robotUpgradeNode = robotUpgradeNode;
            Display();
        }
        public void Display()
        {
            DisplayItemCost();
            mUpgradeButton.onClick.AddListener(OnUpgradeClick);
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(nodeNetwork.Type, nodeNetwork.SubType);

            bool editable = SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME;
            mTitleDropDown.options = robotUpgradeInfo?.GetDropDownOptions();
            mTitleDropDown.value = robotUpgradeNode.NodeData.UpgradeType;
            mTitleDropDown.interactable = editable;
            if (editable)
            {
                mTitleDropDown.GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);
                Image[] images = mTitleDropDown.GetComponentsInChildren<Image>();
                foreach (Image image in images)
                {
                    image.gameObject.SetActive(true);
                }
            }
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
