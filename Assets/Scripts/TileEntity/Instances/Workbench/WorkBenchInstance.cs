using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Newtonsoft.Json;
using Recipe.Processor;
using TileEntity.Instances.WorkBench;

namespace TileEntity.Instances.WorkBenchs {
    public class WorkBenchInstance : TileEntityInstance<WorkBench>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable
    {
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
            return JsonConvert.SerializeObject(WorkBenchData);
        }

        public void unserialize(string data)
        {
            WorkBenchData = JsonConvert.DeserializeObject<WorkBenchData>(data);
        }

        public void PlaceInitialize()
        {
            WorkBenchData = new WorkBenchData(string.Empty, new List<int>(), -1,-1,-1);
        }
    }
    public class WorkBenchData
    {
        public string CurrentSearch;
        public List<int> HiddenModes;
        public int WhiteListedMode;
        public int SelectedIndex;
        public int SelectedMode;

        public WorkBenchData(string currentSearch, List<int> hiddenModes, int whiteListedMode, int selectedIndex, int selectedMode)
        {
            this.CurrentSearch = currentSearch;
            this.HiddenModes = hiddenModes;
            this.WhiteListedMode = whiteListedMode;
            this.SelectedIndex = selectedIndex;
            this.SelectedMode = selectedMode;
        }
    }
}

