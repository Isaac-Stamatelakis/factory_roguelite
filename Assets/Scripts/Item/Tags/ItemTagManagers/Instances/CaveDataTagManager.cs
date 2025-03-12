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
    public class CaveDataTagManager : ItemTagManager, IToolTipTagViewable, IItemTagStackable
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

        public bool AreStackable(object first, object second)
        {
            if (first is not string firstStr || second is not string secondStr) return false;
            return string.Equals(firstStr, secondStr);
        }
    }
}
