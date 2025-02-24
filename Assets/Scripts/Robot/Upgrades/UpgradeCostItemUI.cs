using DevTools;
using Item.Slot;
using Items;
using Items.Inventory;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Robot.Upgrades
{
    internal class UpgradeCostItemUI : ItemSlotUI, IPointerClickHandler, IItemListReloadable
    {
        private RobotUpgradeNodeContentUI robotUpgradeNodeContentUI;
        private uint gottenAmount;
        private int index;
        private RobotUpgradeNode robotUpgradeNode;
        public void Initialize(RobotUpgradeNode robotUpgradeNode, int index, RobotUpgradeNodeContentUI robotUpgradeNodeContentUI) {
            this.robotUpgradeNodeContentUI = robotUpgradeNodeContentUI;
            this.robotUpgradeNode = robotUpgradeNode;
            this.index = index;
            reload();
        }
        public override void SetAmountText()
        {
            
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(robotUpgradeNode.NodeData.Cost[index]);
            uint requiredAmount = RobotUpgradeUtils.GetRequireAmountMultiplier(robotUpgradeNode) * itemSlot.amount;
            if (robotUpgradeNode.IsCompleted())
            {
                gottenAmount = requiredAmount;
            }
            mBottomText.color = gottenAmount >= requiredAmount ? Color.green : Color.red;
            mBottomText.text = ItemDisplayUtils.FormatAmountText(gottenAmount,oneInvisible:false) + "/" +
                               ItemDisplayUtils.FormatAmountText(requiredAmount,oneInvisible:false);
 
        }

        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (SceneManager.GetActiveScene().name != DevToolUtils.SCENE_NAME) return;
            SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(robotUpgradeNodeContentUI.ItemSlotEditorUIPrefab);
            serializedItemSlotEditorUI.Init(robotUpgradeNodeContentUI.RobotUpgradeNode.NodeData.Cost,index,this,gameObject,displayTags:false);
            serializedItemSlotEditorUI.transform.SetParent(robotUpgradeNodeContentUI.transform.parent,false);
        }

        public void reload()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(robotUpgradeNode.NodeData.Cost[index]);
            
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            
            if (!ReferenceEquals(PlayerManager.Instance, null))
            {
                gottenAmount = ItemSlotUtils.AmountOf(itemSlot, PlayerManager.Instance.GetPlayer().PlayerInventory.Inventory);
            }
            
            gottenAmount = GlobalHelper.Clamp(gottenAmount,0,itemSlot.amount);
            Display(itemSlot);
        }

        public void reloadAll()
        {
            robotUpgradeNodeContentUI.DisplayItemCost();
        }
    }
}
