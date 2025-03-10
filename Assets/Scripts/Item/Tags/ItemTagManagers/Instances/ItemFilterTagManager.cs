using System.Collections.Generic;
using System.ComponentModel.Design;
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
using UnityEditor.Experimental;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class ItemFilterTagManager : ItemTagManager, IItemTagReferencedType
    {
        public override string Serialize(object obj)
        {
            return obj is not ItemFilter itemFilter ? null : JsonConvert.SerializeObject(itemFilter);
        }

        public override object Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<ItemFilter>(data);
        }

        public object CreateDeepCopy(object obj)
        {
            if (obj is not ItemFilter itemFilter) return null;
            List<string> idCopy = new List<string>();
            foreach (string id in itemFilter.ids)
            {
                idCopy.Add(id);
            }

            return new ItemFilter(idCopy,itemFilter.whitelist);
        }
    }
}
