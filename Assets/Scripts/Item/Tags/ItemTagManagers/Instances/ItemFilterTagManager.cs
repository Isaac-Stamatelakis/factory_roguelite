using System;
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
    public class ItemFilterTagManager : ItemTagManager, IItemTagReferencedType, IItemTagStackable
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

        public bool AreStackable(object first, object second)
        {
            if (first is not ItemFilter firstFilter) return false;
            if (second is not ItemFilter secondFilter) return false;
            if (firstFilter.ids.Count != secondFilter.ids.Count) return false;
            if (firstFilter.whitelist != secondFilter.whitelist) return false;
            for (int i = 0; i < firstFilter.ids.Count; i++)
            {
                if (!String.Equals(firstFilter.ids[i], secondFilter.ids[i])) return false;
            }
            return true;
        }
    }
}
