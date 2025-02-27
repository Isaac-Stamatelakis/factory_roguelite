using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using Newtonsoft.Json;
using UI;
using UI.QuestBook;
using UnityEngine;

namespace TileEntity.Instances.Caves.Researcher {
    public class CaveProcessorInstance : TileEntityInstance<CaveProcessor>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable, ITickableTileEntity, IEnergyConduitInteractable, IConduitPortTileEntity, IInventoryListener
    {
        public List<ItemSlot> InputDrives;
        public List<ItemSlot> OutputDrives;
        public ulong Energy;
        public bool ResearchProgressing;
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

        public void OnRightClick()
        {
            CaveProcessorUI caveProcessorUI = GameObject.Instantiate(TileEntityObject.uIManager.getUIElement()).GetComponent<CaveProcessorUI>();
            caveProcessorUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(caveProcessorUI.gameObject);
            CacheQuestData();
        }

        public string Serialize()
        {
            SerializedData serializedData = new SerializedData(
                Energy, 
                ItemSlotFactory.seralizeItemSlot(InputDrives[0]), 
                ItemSlotFactory.seralizeItemSlot(OutputDrives[0]),
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
            Energy = serializedData.Energy;
            
            ItemSlot inputSlot = ItemSlotFactory.DeserializeSlot(serializedData.InputItem);
            InputDrives = new List<ItemSlot> { inputSlot };
            
            ItemSlot outputSlot = ItemSlotFactory.DeserializeSlot(serializedData.OutputItem);
            OutputDrives = new List<ItemSlot> { outputSlot };
            
            researchedCaves = serializedData.ResearchedCaves;
            ResearchDriveProcess = serializedData.ResearchDriveProcess;
            CopyDriveProcess = serializedData.CopyDriveProcess;
            CurrentlyCopyingCave = serializedData.CopyCaveId;

        }

        public void PlaceInitialize()
        {
            InputDrives = ItemSlotFactory.createEmptyInventory(DRIVE_SPACE);
            OutputDrives = ItemSlotFactory.createEmptyInventory(DRIVE_SPACE);
            
            researchedCaves = new List<string>{DEFAULT_CAVE_NAME};
        }

        public void TickUpdate()
        {
            CopyCaveTickUpdate();
            ResearchCaveTickUpdate();

        }

        private void ResearchCaveTickUpdate()
        {
            if (ResearchDriveProcess == null) return;
            ulong costPerTick = ResearchDriveProcess.EnergyCostPerTick;
            if (Energy < costPerTick) // Unlock most machines if the user cannot supply enough energy to progress the drive they lose progress
            {
                ResearchProgressing = false;
                if (ResearchDriveProcess.Energy < costPerTick)
                {
                    ResearchDriveProcess.Energy = 0;
                }
                else
                {
                    ResearchDriveProcess.Energy -= costPerTick;
                }
                return;
            }
            
            ResearchProgressing = ResearchDriveProcess.Energy > costPerTick;
            ResearchDriveProcess.Energy += costPerTick;
            Energy -= costPerTick;
            
            
            if (!ResearchDriveProcess.Complete) return;
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

        public ulong InsertEnergy(ulong energy, Vector2Int portPosition)
        {
            if (ResearchDriveProcess == null) return 0;
            ulong maxEnergy = ResearchDriveProcess.EnergyCostPerTick;
            ulong sum = Energy+=energy;
            if (sum > maxEnergy) {
                Energy = maxEnergy;
                return sum - maxEnergy;
            }
            Energy = sum;
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
            public List<string> ResearchedCaves;
            public ResearchDriveProcess ResearchDriveProcess;
            public CopyDriveProcess CopyDriveProcess;
            public string CopyCaveId;

            public SerializedData(ulong energy, string inputItem, string outputItem, 
                List<string> researchedCaves, ResearchDriveProcess researchDriveProcess, CopyDriveProcess copyDriveProcess, string copyCaveId)
            {
                Energy = energy;
                InputItem = inputItem;
                OutputItem = outputItem;
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
        public ulong Energy;
        public ulong EnergyCostPerTick;
        public ulong Cost;
        public string ResearchId;
        public bool Complete => Energy >= Cost;
        public float Progress => (float)Energy / Cost;
        public ResearchDriveProcess(Tier tier, string researchId)
        {
            Cost = tier == Tier.Basic ? 4096 : 16000 * GlobalHelper.BinaryExponentiation(4,(int)tier+1);
            EnergyCostPerTick =  GetMinimumEnergy(tier);
            ResearchId = researchId;
        }

        /// <summary>
        /// Returns the minimum energy required to research a cave
        /// </summary>
        /// <param name="tier"></param>
        /// <example>4, 128, 512, 2048, ...</example>
        /// <remarks>Basic tier has a very low energy cost for SMRs to work</remarks>
        /// <returns></returns>
        private ulong GetMinimumEnergy(Tier tier)
        {
            if (tier == Tier.Basic) return 4;
            return 8 * GlobalHelper.BinaryExponentiation(4,(int)(tier+1));
            
        }
        
        
    }
}

