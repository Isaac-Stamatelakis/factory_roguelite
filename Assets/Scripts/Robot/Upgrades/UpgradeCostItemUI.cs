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
        private SerializedItemSlot serializedItemSlot;
        private uint gottenAmount;
        private int index;
        public void Initialize(SerializedItemSlot serializedItemSlot, int index, RobotUpgradeNodeContentUI robotUpgradeNodeContentUI) {
            this.robotUpgradeNodeContentUI = robotUpgradeNodeContentUI;
            this.serializedItemSlot = serializedItemSlot;
            this.index = index;
            reload();
        }
        public override void SetAmountText()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(serializedItemSlot);
            mBottomText.color = gottenAmount >= itemSlot.amount ? Color.green : Color.red;
            mBottomText.text = ItemDisplayUtils.FormatAmountText(gottenAmount,oneInvisible:false) + "/" +
                               ItemDisplayUtils.FormatAmountText(itemSlot.amount,oneInvisible:false);
 
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (SceneManager.GetActiveScene().name != DevToolUtils.SCENE_NAME) return;
            SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(robotUpgradeNodeContentUI.ItemSlotEditorUIPrefab);
            serializedItemSlotEditorUI.Init(robotUpgradeNodeContentUI.RobotUpgradeNode.NodeData.Cost,index,this,gameObject);
            CanvasController.Instance.DisplayObject(serializedItemSlotEditorUI.gameObject);
        }

        public void reload()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(serializedItemSlot);
            
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
            
        }
    }
}
