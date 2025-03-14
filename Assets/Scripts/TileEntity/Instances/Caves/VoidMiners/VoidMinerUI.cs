using Items.Inventory;
using UnityEngine;

namespace TileEntity.Instances.Caves.VoidMiners
{
    public class VoidMinerUI : MonoBehaviour, ITileEntityUI<VoidMinerInstance>
    {
        [SerializeField] private InventoryUI mDriveInventoryUI;
        [SerializeField] private InventoryUI mFilterInventoryUI;
        [SerializeField] private InventoryUI mUpgradeInventoryUI;
        [SerializeField] private InventoryUI mStoneOutputUI;
        [SerializeField] private InventoryUI mOreOutputUI;
        [SerializeField] private InventoryUI mFluidOutputUI;
        
        public void DisplayTileEntityInstance(VoidMinerInstance tileEntityInstance)
        {
            
        }
    }
}
