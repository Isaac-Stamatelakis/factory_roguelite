using System.Collections.Generic;
using System.IO;
using Conduits.Ports;
using Item.Slot;
using Items;
using Items.Tags;
using Newtonsoft.Json;
using RobotModule;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;
using TileEntity.Instances.Matrix;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class ItemFilterTagManager : ItemTagManager
    {
        public override string Serialize(object obj)
        {
            return obj is not ItemFilter itemFilter ? null : JsonConvert.SerializeObject(itemFilter);
        }

        public override object Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<ItemFilter>(data);
        }

    }
}
