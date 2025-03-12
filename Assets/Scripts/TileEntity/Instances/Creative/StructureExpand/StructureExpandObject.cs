using System;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using Newtonsoft.Json;
using TileEntity.Instances.CompactMachine.BluePrinter;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace TileEntity.Instances.Creative.CreativeChest
{
    [CreateAssetMenu(fileName = "New Structure Expand", menuName = "Tile Entity/Creative/Structure Expand")]
    public class StructureExpandObject : TileEntityObject
    {
        public AssetReference StructureExpandUIPrefab;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new StructureExpandInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class StructureExpandInstance : TileEntityInstance<StructureExpandObject>, ISerializableTileEntity, IRightClickableTileEntity, IPlaceInitializable
    {
        internal StructureExpandData StructureExpandData;
        public StructureExpandInstance(StructureExpandObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(StructureExpandData);
        }

        public void Unserialize(string data)
        {
            StructureExpandData = JsonConvert.DeserializeObject<StructureExpandData>(data);
        }

        public void OnRightClick()
        {
            // Testing with asset reference;
            Addressables.LoadAssetAsync<GameObject>(tileEntityObject.StructureExpandUIPrefab).Completed += OnComplete;
        }

        private void OnComplete(AsyncOperationHandle<GameObject> handle)
        {
            GameObject loadedObject = handle.Result;
            StructureExpandUI expandUI = GameObject.Instantiate(loadedObject).GetComponent<StructureExpandUI>();
            expandUI.DisplayTileEntityInstance(this);
            CanvasController.Instance.DisplayObject(expandUI.gameObject);
            Addressables.Release(handle);
        }

        public void PlaceInitialize()
        {
            StructureExpandData = new StructureExpandData();
        }
    }
    internal class StructureExpandData
    {
        public string Id;
        public int MaxSize = 128;
    }
}
