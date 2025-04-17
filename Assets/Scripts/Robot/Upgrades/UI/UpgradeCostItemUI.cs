using DevTools;
using Item.Slot;
using Items;
using Items.Inventory;
using Robot.Upgrades.Network;
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
            uint multiplier = RobotUpgradeUtils.GetRequireAmountMultiplier(robotUpgradeNode);
            itemSlot.amount *= multiplier;
            uint requiredAmount = itemSlot.amount;
            if (robotUpgradeNode.IsCompleted())
            {
                float newAmount = requiredAmount / robotUpgradeNode.NodeData.CostMultiplier;
                requiredAmount = (uint)newAmount; // Divide by amount since otherwise it will show cost of next upgrade instead of last, should probably cahnge this latter
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
            serializedItemSlotEditorUI.Initialize(robotUpgradeNodeContentUI.RobotUpgradeNode.NodeData.Cost,index,this,gameObject,displayTags:false);
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
            uint multiplier = RobotUpgradeUtils.GetRequireAmountMultiplier(robotUpgradeNode);
            gottenAmount = GlobalHelper.Clamp(gottenAmount,0,itemSlot.amount*multiplier);
            Display(itemSlot);
        }

        public void reloadAll()
        {
            robotUpgradeNodeContentUI.DisplayItemCost();
        }
    }
}
