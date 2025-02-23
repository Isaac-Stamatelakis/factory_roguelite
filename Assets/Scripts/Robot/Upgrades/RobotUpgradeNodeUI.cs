using Item.Slot;
using Items;
using Microsoft.Unity.VisualStudio.Editor;
using UI.NodeNetwork;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Robot.Upgrades
{
    public class RobotUpgradeNodeUI : NodeUI<RobotUpgradeNode,RobotUpgradeNetworkUI>
    {
        [SerializeField] private Image mIconImage;
        
        
        protected override void openContent()
        {
            Debug.Log("Clicked");
        }
        
        public override void DisplayImage()
        {
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(node.NodeData?.IconItemId);
            mIconImage.sprite = itemObject?.getSprite();
        }
    }
}
