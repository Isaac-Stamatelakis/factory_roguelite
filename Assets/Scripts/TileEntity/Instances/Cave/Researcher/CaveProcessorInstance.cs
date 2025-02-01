using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using Newtonsoft.Json;
using UI;
using UnityEngine;

namespace TileEntity.Instances {
    public class CaveProcessorInstance : TileEntityInstance<CaveProcessor>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable, ITickableTileEntity, IEnergyConduitInteractable, IConduitPortTileEntity, IInventoryListener
    {
        public List<ItemSlot> InputDrives;
        public List<ItemSlot> OutputDrives;
        private ulong Energy;
        private List<string> researchedCaves;
        
        private string currentlyCopyingCave;
        internal ResearchDriveProcess ResearchDriveProcess;
        internal CopyDriveProcess CopyDriveProcess;
        private const string DRIVE_ID = "cave_data_drive";
       
        private const int DRIVE_SPACE = 1;
        public CaveProcessorInstance(CaveProcessor tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            CaveProcessorUI caveProcessorUI = GameObject.Instantiate(TileEntityObject.uIManager.getUIElement()).GetComponent<CaveProcessorUI>();
            caveProcessorUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(caveProcessorUI.gameObject);
        }

        public string Serialize()
        {
            SerializedData serializedData = new SerializedData(
                Energy, 
                ItemSlotFactory.seralizeItemSlot(InputDrives[0]), 
                ItemSlotFactory.seralizeItemSlot(OutputDrives[0])
            );
            return JsonConvert.SerializeObject(serializedData);
        }

        public void Unserialize(string data)
        {
            SerializedData serializedData = JsonConvert.DeserializeObject<SerializedData>(data);
            Energy = serializedData.Energy;
            
            ItemSlot inputSlot = ItemSlotFactory.DeserializeSlot(serializedData.InputItem);
            InputDrives = new List<ItemSlot> { inputSlot };
            
            ItemSlot outputSlot = ItemSlotFactory.DeserializeSlot(serializedData.OutputItem);
            OutputDrives = new List<ItemSlot> { outputSlot };
            
        }

        public void PlaceInitialize()
        {
            InputDrives = ItemSlotFactory.createEmptyInventory(DRIVE_SPACE);
            OutputDrives = ItemSlotFactory.createEmptyInventory(DRIVE_SPACE);
            researchedCaves = new List<string>{"icy_caverns"};
        }

        public void TickUpdate()
        {
            CopyCaveTickUpdate();
            
        }

        private void ResearchCaveTickUpdate()
        {
            if (ResearchDriveProcess == null) return;
            
        }

        private void CopyCaveTickUpdate()
        {
            if (CopyDriveProcess == null) return;
            
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
            InventoryUpdate(0);
        }
        
        public void InventoryUpdate(int n)
        {
            if (CopyDriveProcess is { IsComplete: false }) return;

            if (!CanBeginCopyProcess()) return;
            InputDrives[0].amount--;
            
            CopyDriveProcess ??= new CopyDriveProcess();
            CopyDriveProcess.CopyId = currentlyCopyingCave;
            CopyDriveProcess.Ticks = 0;

        }

        private bool CanBeginCopyProcess()
        {
            if (currentlyCopyingCave == null) return false;
            
            ItemSlot input = InputDrives[0];
            if (ItemSlotUtils.IsItemSlotNull(input)) return false;
            
            ItemSlot output = OutputDrives[0];
            if (ItemSlotUtils.IsItemSlotNull(output)) return true;
            if (output.amount >= Global.MaxSize) return false;
            string outputCopyId = output.tags?.Dict?[ItemTag.CaveData] as string;
            return outputCopyId == null || outputCopyId == currentlyCopyingCave;
        }

        public ulong InsertEnergy(ulong energy, Vector2Int portPosition)
        {
            return energy;
        }

        public ref ulong GetEnergy(Vector2Int portPosition)
        {
            return ref Energy;
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitLayout;
        }

        private class SerializedData
        {
            public ulong Energy;
            public string InputItem;
            public string OutputItem;

            public SerializedData(ulong energy, string inputItem, string outputItem)
            {
                Energy = energy;
                InputItem = inputItem;
                OutputItem = outputItem;
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
        public ulong Energy;
        public Tier Tier;
        public string ResearchId;

        public ResearchDriveProcess(Tier tier, string researchId)
        {
            Tier = tier;
            ResearchId = researchId;
        }
    }
}

