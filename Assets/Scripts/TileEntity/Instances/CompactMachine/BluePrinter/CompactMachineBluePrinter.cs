using System.Collections.Generic;
using Chunks;
using Item.Slot;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TileEntity.Instances.CompactMachine.BluePrinter
{
    
    public class CompactMachineBluePrinterInstance : TileEntityInstance<CompactMachineBluePrinterObject>, IRightClickableTileEntity, ISerializableTileEntity, IPlaceInitializable
    {
        public const int DEFAULT_INVENTORY_SIZE = 30;
        private CompactMachineBluePrintInventory bluePrintInventory;
        internal CompactMachineBluePrintInventory BluePrintInventory => bluePrintInventory;
        public CompactMachineBluePrinterInstance(CompactMachineBluePrinterObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            // Testing with asset reference;
            Addressables.LoadAssetAsync<GameObject>(tileEntityObject.UIAssetReference).Completed += OnComplete;
        }

        private void OnComplete(AsyncOperationHandle<GameObject> handle)
        {
            GameObject loadedObject = handle.Result;
            CompactMachineBluePrinterUI bluePrinterUI = GameObject.Instantiate(loadedObject).GetComponent<CompactMachineBluePrinterUI>();
            bluePrinterUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(bluePrinterUI.gameObject);
            Addressables.Release(handle);
        }
        
        public void PlaceInitialize()
        {
            bluePrintInventory = new CompactMachineBluePrintInventory(
                compactMachineInput: ItemSlotUtils.CreateNullInventory(1),
                compactMachineOutput: ItemSlotUtils.CreateNullInventory(1),
                itemInput: ItemSlotUtils.CreateNullInventory(DEFAULT_INVENTORY_SIZE),
                null
            );
        }

        public string Serialize()
        {
            SerializedCompactMachineBluePrintInventory serializedCompactMachineBluePrintInventory =
                new SerializedCompactMachineBluePrintInventory(
                    ItemSlotFactory.serializeList(bluePrintInventory.CompactMachineInput),
                    ItemSlotFactory.serializeList(bluePrintInventory.CompactMachineOutput),
                    ItemSlotFactory.serializeList(bluePrintInventory.ItemInput),
                    bluePrintInventory.CurrentHash
                );
            return JsonConvert.SerializeObject(serializedCompactMachineBluePrintInventory);
        }

        public void Unserialize(string data)
        {
            SerializedCompactMachineBluePrintInventory serializedCompactMachineBluePrintInventory = JsonConvert.DeserializeObject<SerializedCompactMachineBluePrintInventory>(data);
            bluePrintInventory = new CompactMachineBluePrintInventory(
                ItemSlotFactory.Deserialize(serializedCompactMachineBluePrintInventory.CompactMachineInput),
                ItemSlotFactory.Deserialize(serializedCompactMachineBluePrintInventory.CompactMachineOutput),
                ItemSlotFactory.Deserialize(serializedCompactMachineBluePrintInventory.ItemInput),
                serializedCompactMachineBluePrintInventory.CurrentHash
            );
        }
    }
    
    internal class CompactMachineBluePrintInventory
    {
        public string CurrentHash;
        public List<ItemSlot> CompactMachineInput;
        public List<ItemSlot> CompactMachineOutput;
        public List<ItemSlot> ItemInput;

        public CompactMachineBluePrintInventory(List<ItemSlot> compactMachineInput, List<ItemSlot> compactMachineOutput, List<ItemSlot> itemInput, string currentHash)
        {
            CompactMachineInput = compactMachineInput;
            CompactMachineOutput = compactMachineOutput;
            ItemInput = itemInput;
            CurrentHash = currentHash;
        }
    }

    internal class SerializedCompactMachineBluePrintInventory
    {
        public string CompactMachineInput;
        public string CompactMachineOutput;
        public string ItemInput;
        public string CurrentHash;

        public SerializedCompactMachineBluePrintInventory(string compactMachineInput, string compactMachineOutput, string itemInput, string currentHash)
        {
            CompactMachineInput = compactMachineInput;
            CompactMachineOutput = compactMachineOutput;
            ItemInput = itemInput;
            CurrentHash = currentHash;
        }
    }
}
