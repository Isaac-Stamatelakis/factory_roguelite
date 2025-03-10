using System.Collections.Generic;
using System.IO;
using Item.Slot;
using Items;
using Items.Tags;
using RobotModule;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;
using TileEntity.Instances.Matrix;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class RobotTagManager : ItemTagManager
    {
        public override string Serialize(object obj)
        {
            return obj is not RobotItemData robotItemData ? null : RobotDataFactory.Serialize(robotItemData);
        }

        public override object Deserialize(string data)
        {
            return RobotDataFactory.Deserialize(data);
        }

    }
}
