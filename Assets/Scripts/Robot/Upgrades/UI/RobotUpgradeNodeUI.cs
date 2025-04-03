using Item.Slot;
using Items;
using Microsoft.Unity.VisualStudio.Editor;
using Robot.Upgrades.Network;
using UI.NodeNetwork;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Robot.Upgrades
{
    public class RobotUpgradeNodeUI : NodeUI<RobotUpgradeNode,RobotUpgradeNetworkUI>
    {
        protected override void openContent()
        {
            nodeNetwork.RobotUpgradeUI.DisplayNodeContent(node);
        }
        
        public override void DisplayImage()
        {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(node.NodeData?.IconItemId);
            if (!itemObject) return;
            mItemSlotUI.Display(new ItemSlot(itemObject,1,null));
        }
    }
}
