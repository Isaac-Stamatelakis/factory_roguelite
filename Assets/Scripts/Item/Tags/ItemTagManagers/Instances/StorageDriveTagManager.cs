using System.Collections.Generic;
using System.IO;
using Item.Slot;
using Items.Tags;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class StorageDriveTagManager : ItemTagManager
    {
        public override string Serialize(object obj)
        {
            return obj is not List<ItemSlot> inventory ? null : ItemSlotFactory.serializeList(inventory);
        }

        public override object Deserialize(string data)
        {
            return ItemSlotFactory.Deserialize(data);
        }

    }
}
