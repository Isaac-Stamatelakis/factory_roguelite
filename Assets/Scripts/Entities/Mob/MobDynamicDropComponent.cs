using System;
using System.Collections.Generic;
using Entities.Mobs;
using Item.Slot;
using Newtonsoft.Json;
using UnityEngine;

namespace Entities.Mob
{
    public class MobDynamicDropComponent : MonoBehaviour, ISerializableMobComponent
    {
        public List<ItemSlot> Drops { get; private set; }

        public SerializableMobComponentType ComponentType => SerializableMobComponentType.AdditionalDrops;

        public string Serialize()
        {
            return ItemSlotFactory.serializeList(Drops);
        }

        public void Deserialize(string data)
        {
            if (data == null) return;
            try
            {
                Drops = ItemSlotFactory.Deserialize(data);
            }
            catch (Exception e) when (e is ArgumentException or JsonSerializationException or JsonReaderException)
            {
                Drops = null;
            }
        }

        public void SetDrops(List<ItemSlot> drops)
        {
            this.Drops = drops;
        }
    }
}
