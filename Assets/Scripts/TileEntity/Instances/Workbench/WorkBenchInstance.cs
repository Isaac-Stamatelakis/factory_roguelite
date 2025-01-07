using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Item.Slot;
using Newtonsoft.Json;
using Recipe.Processor;
using TileEntity.Instances.WorkBench;

namespace TileEntity.Instances.WorkBenchs {
    public class WorkBenchInstance : TileEntityInstance<WorkBench>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable
    {
        public List<ItemSlot> Inventory;
        public WorkBenchData WorkBenchData;
        public WorkBenchInstance(WorkBench tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void onRightClick()
        {
            tileEntityObject.UIAssetManager.display<WorkBenchInstance,WorkBenchUI>(this);
        }

        public string serialize()
        {
            WorkBenchData.SInventory = ItemSlotFactory.serializeList(Inventory);
            return JsonConvert.SerializeObject(WorkBenchData);
        }

        public void unserialize(string data)
        {
            WorkBenchData = JsonConvert.DeserializeObject<WorkBenchData>(data);
            Inventory = ItemSlotFactory.Deserialize(WorkBenchData.SInventory);
            if (Inventory == null && tileEntityObject.HasInventory)
            {
                Inventory = ItemSlotFactory.createEmptyInventory(10);
            }
        }

        public void PlaceInitialize()
        {
            WorkBenchData = new WorkBenchData(string.Empty, new List<int>(), -1,-1,-1,null);
            if (tileEntityObject.HasInventory) Inventory = ItemSlotFactory.createEmptyInventory(10);
        }
    }
    public class WorkBenchData
    {
        public string CurrentSearch;
        public List<int> HiddenModes;
        public int WhiteListedMode;
        public int SelectedIndex;
        public int SelectedMode;
        public string SInventory;

        public WorkBenchData(string currentSearch, List<int> hiddenModes, int whiteListedMode, int selectedIndex, int selectedMode, string sInventory)
        {
            this.CurrentSearch = currentSearch;
            this.HiddenModes = hiddenModes;
            this.WhiteListedMode = whiteListedMode;
            this.SelectedIndex = selectedIndex;
            this.SelectedMode = selectedMode;
            this.SInventory = sInventory;
        }
    }
}

