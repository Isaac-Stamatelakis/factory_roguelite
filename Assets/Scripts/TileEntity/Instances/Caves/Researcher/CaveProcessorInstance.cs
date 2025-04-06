using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Entities;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using Newtonsoft.Json;
using UI;
using UI.QuestBook;
using UnityEngine;

namespace TileEntity.Instances.Caves.Researcher {
    public class CaveProcessorInstance : TileEntityInstance<CaveProcessor>, ISerializableTileEntity, IPlaceInitializable, ITickableTileEntity, IConduitPortTileEntity, IInventoryListener, IBreakActionTileEntity
    {
        public List<ItemSlot> InputDrives;
        public List<ItemSlot> OutputDrives;
        public List<ItemSlot> ResearchItems;
        private List<string> researchedCaves;
        public List<string> ResearchedCaves => researchedCaves;
        public string CurrentlyCopyingCave;
        internal ResearchDriveProcess ResearchDriveProcess;
        internal CopyDriveProcess CopyDriveProcess;
        private const string DRIVE_ID = "cave_data_drive";
        private const string DEFAULT_CAVE_NAME = "Icy_Caverns";
       
        private const int DRIVE_SPACE = 1;
        public CaveProcessorInstance(CaveProcessor tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public string Serialize()
        {
            SerializedData serializedData = new SerializedData(
                ItemSlotFactory.seralizeItemSlot(InputDrives[0]), 
                ItemSlotFactory.seralizeItemSlot(OutputDrives[0]),
                ItemSlotFactory.serializeList(ResearchItems),
                researchedCaves,
                ResearchDriveProcess,
                CopyDriveProcess,
                CurrentlyCopyingCave
            );
            return JsonConvert.SerializeObject(serializedData);
        }

        public void Unserialize(string data)
        {
            SerializedData serializedData = JsonConvert.DeserializeObject<SerializedData>(data);
            
            ItemSlot inputSlot = ItemSlotFactory.DeserializeSlot(serializedData.InputItem);
            InputDrives = new List<ItemSlot> { inputSlot };
            
            ItemSlot outputSlot = ItemSlotFactory.DeserializeSlot(serializedData.OutputItem);
            OutputDrives = new List<ItemSlot> { outputSlot };
            
            ResearchItems =  ItemSlotFactory.Deserialize(serializedData.ResearchItems);
            
            researchedCaves = serializedData.ResearchedCaves;
            ResearchDriveProcess = serializedData.ResearchDriveProcess;
            CopyDriveProcess = serializedData.CopyDriveProcess;
            CurrentlyCopyingCave = serializedData.CopyCaveId;

        }

        public void PlaceInitialize()
        {
            InputDrives = ItemSlotFactory.createEmptyInventory(DRIVE_SPACE);
            OutputDrives = ItemSlotFactory.createEmptyInventory(DRIVE_SPACE);
            ResearchItems = new List<ItemSlot>();
            researchedCaves = new List<string>{DEFAULT_CAVE_NAME};
        }

        public void TickUpdate()
        {
            CopyCaveTickUpdate();
            ResearchCaveTickUpdate();

        }

        private void ResearchCaveTickUpdate()
        {
            if (ResearchDriveProcess == null || !ResearchDriveProcess.Satisfied) return;
            ResearchDriveProcess.Progress += Time.fixedDeltaTime * 1 / 10f;
            
            if (ResearchDriveProcess.Progress < 1f) return;
            researchedCaves.Add(ResearchDriveProcess.ResearchId);
            CacheQuestData();
            ResearchDriveProcess = null;
        }

        private void CacheQuestData()
        {
            string allData = string.Empty;
            foreach (string cave in researchedCaves)
            {
                allData += cave + "_";
            }
            PlayerManager.Instance.GetPlayer().QuestBookCache.CacheQuestData(allData, QuestTaskType.Dimension);
        }

        private void CopyCaveTickUpdate()
        {
            if (CopyDriveProcess?.CopyId == null) return;
            
            CopyDriveProcess.Ticks++;
            if (!CopyDriveProcess.IsComplete) return;
            
            // Note: Before the process began it was ensured that nothing goes wrong in output
            ItemSlot output = OutputDrives[0];
            if (ItemSlotUtils.IsItemSlotNull(output))
            {
                ItemSlot itemSlot = new ItemSlot(ItemRegistry.GetInstance().GetItemObject(DRIVE_ID), 1, new ItemTagCollection(new Dictionary<ItemTag, object>()));
                ItemSlotUtils.AddTag(itemSlot,ItemTag.CaveData,CopyDriveProcess.CopyId);
                OutputDrives[0] = itemSlot;
            }
            else
            {
                output.amount++;
            }

            CopyDriveProcess.CopyId = null;
            InventoryUpdate(0);
        }
        
        public void InventoryUpdate(int n)
        {
            if (CopyDriveProcess is { IsComplete: false }) return;

            if (!CanBeginCopyProcess()) return;
            
            InputDrives[0].amount--;
            
            CopyDriveProcess ??= new CopyDriveProcess();
            CopyDriveProcess.CopyId = CurrentlyCopyingCave;
            CopyDriveProcess.Ticks = 0;

        }

        private bool CanBeginCopyProcess()
        {
            if (CurrentlyCopyingCave == null) return false;
            
            ItemSlot input = InputDrives[0];
            if (ItemSlotUtils.IsItemSlotNull(input)) return false;
            
            ItemSlot output = OutputDrives[0];
            if (ItemSlotUtils.IsItemSlotNull(output)) return true;
            if (output.amount >= Global.MAX_SIZE) return false;
            string outputCopyId = output.tags?.Dict?[ItemTag.CaveData] as string;
            return outputCopyId == null || outputCopyId == CurrentlyCopyingCave;
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitLayout;
        }
        
        public void OnBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk) return;
            Vector2 position = GetWorldPosition();
            foreach (ItemSlot itemSlot in InputDrives)
            {
                ItemEntityFactory.SpawnItemEntity(position, itemSlot, loadedChunk.GetEntityContainer());
            }
            foreach (ItemSlot itemSlot in OutputDrives)
            {
                ItemEntityFactory.SpawnItemEntity(position, itemSlot, loadedChunk.GetEntityContainer());
            }
        }

        private class SerializedData
        {
            public string InputItem;
            public string OutputItem;
            public string ResearchItems;
            public List<string> ResearchedCaves;
            public ResearchDriveProcess ResearchDriveProcess;
            public CopyDriveProcess CopyDriveProcess;
            public string CopyCaveId;

            public SerializedData(string inputItem, string outputItem, string researchItems, 
                List<string> researchedCaves, ResearchDriveProcess researchDriveProcess, CopyDriveProcess copyDriveProcess, string copyCaveId)
            {
                InputItem = inputItem;
                OutputItem = outputItem;
                ResearchItems = researchItems;
                ResearchedCaves = researchedCaves;
                ResearchDriveProcess = researchDriveProcess;
                CopyDriveProcess = copyDriveProcess;
                CopyCaveId = copyCaveId;
            }
        }
    }
    
    internal class CopyDriveProcess
    {
        private const int TICKS_TO_COPY = 200;
        public bool IsComplete => Ticks >= TICKS_TO_COPY;
        public string CopyId;
        public uint Ticks;
        public float Progress => (float)Ticks / TICKS_TO_COPY;
    }

    internal class ResearchDriveProcess
    {
        public float Progress;
        public bool Satisfied;
        public string ResearchId;

        public ResearchDriveProcess(float progress, bool satisfied, string researchId)
        {
            Progress = progress;
            Satisfied = satisfied;
            ResearchId = researchId;
        }
    }
}

