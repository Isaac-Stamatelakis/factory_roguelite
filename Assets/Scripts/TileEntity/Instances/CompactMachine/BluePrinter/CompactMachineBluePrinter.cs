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
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/BluePrinter")]
    public class CompactMachineBluePrinterObject : TileEntityObject
    {
        public AssetReference UIAssetReference;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineBluePrinterInstance(this,tilePosition,tileItem,chunk);
        }
        
    }
    public class CompactMachineBluePrinterInstance : TileEntityInstance<CompactMachineBluePrinterObject>, IRightClickableTileEntity, ISerializableTileEntity
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
            CanvasController.Instance.DisplayObject(bluePrinterUI.gameObject);
            Addressables.Release(handle);
        }
        
        public void PlaceInitialize()
        {
            bluePrintInventory = new CompactMachineBluePrintInventory(
                compactMachineInput: ItemSlotUtils.CreateNullInventory(1),
                compactMachineOutput: ItemSlotUtils.CreateNullInventory(1),
                itemInput: ItemSlotUtils.CreateNullInventory(DEFAULT_INVENTORY_SIZE)
            );
        }

        public string Serialize()
        {
            SerializedCompactMachineBluePrintInventory serializedCompactMachineBluePrintInventory =
                new SerializedCompactMachineBluePrintInventory(
                    ItemSlotFactory.serializeList(bluePrintInventory.CompactMachineInput),
                    ItemSlotFactory.serializeList(bluePrintInventory.CompactMachineOutput),
                    ItemSlotFactory.serializeList(bluePrintInventory.ItemInput)
                );
            return JsonConvert.SerializeObject(serializedCompactMachineBluePrintInventory);
        }

        public void Unserialize(string data)
        {
            SerializedCompactMachineBluePrintInventory serializedCompactMachineBluePrintInventory = JsonConvert.DeserializeObject<SerializedCompactMachineBluePrintInventory>(data);
            bluePrintInventory = new CompactMachineBluePrintInventory(
                ItemSlotFactory.Deserialize(serializedCompactMachineBluePrintInventory.CompactMachineInput),
                ItemSlotFactory.Deserialize(serializedCompactMachineBluePrintInventory.CompactMachineOutput),
                ItemSlotFactory.Deserialize(serializedCompactMachineBluePrintInventory.ItemInput)
            );
        }
    }
    
    internal class CompactMachineBluePrintInventory
    {
        public List<ItemSlot> CompactMachineInput;
        public List<ItemSlot> CompactMachineOutput;
        public List<ItemSlot> ItemInput;

        public CompactMachineBluePrintInventory(List<ItemSlot> compactMachineInput, List<ItemSlot> compactMachineOutput, List<ItemSlot> itemInput)
        {
            CompactMachineInput = compactMachineInput;
            CompactMachineOutput = compactMachineOutput;
            ItemInput = itemInput;
        }
    }

    internal class SerializedCompactMachineBluePrintInventory
    {
        public string CompactMachineInput;
        public string CompactMachineOutput;
        public string ItemInput;

        public SerializedCompactMachineBluePrintInventory(string compactMachineInput, string compactMachineOutput, string itemInput)
        {
            CompactMachineInput = compactMachineInput;
            CompactMachineOutput = compactMachineOutput;
            ItemInput = itemInput;
        }
    }
}
