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
    public class CaveDataTagManager : ItemTagManager, IToolTipTagViewable
    {
        public override string Serialize(object obj)
        {
            return obj as string;
        }

        public override object Deserialize(string data)
        {
            return data;
        }

        public string GetToolTip(object obj)
        {
            return $"Data: <b>{obj}</b>";
        }
    }
}
