using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Chunks;
using TileMaps.Type;
using Chunks.Systems;
using TileMaps.Place;
using Tiles;
using Chunks.Partitions;
using Entities;
using Item.Slot;
using Items;

namespace TileMaps {
    public interface IHitableTileMap : IWorldTileMap{
        public bool HitTile(Vector2 position, bool dropItem);
        public bool DeleteTile(Vector2 position);
        public bool BreakAndDropTile(Vector2Int position, bool dropItem);
    }

    public interface IConditionalHitableTileMap : IHitableTileMap
    {
        public bool CanHitTile(int power, Vector2 position);
    }
    public interface IWorldTileMap {
        public void AddPartition(IChunkPartition partition);
        public IEnumerator RemovePartition(Vector2Int partitionPosition);
        public bool ContainsPartition(Vector2Int partitionPosition);
        public void PlaceNewTileAtLocation(int x, int y, ItemObject itemObject);
        public void PlaceItemTileAtLocation(Vector2Int partition, Vector2Int partitionPosition, ItemObject itemObject);
        public TileMapType GetTileMapType();
        public Vector2Int WorldToTileMapPosition(Vector2 worldPosition);
        public bool HasTile(Vector2Int position);
        public void RemoveForSwitch(Vector2Int position);
        public void PlaceTileAtLocation(Vector2Int position, TileBase tileBase);
        public void AddListener(ITileMapListener listener);
        public void SetHighlight(bool on);
        public Tilemap GetTilemap();
        public void Initialize(TileMapType type);
        public ClosedChunkSystem GetSystem();
        public void BreakTile(Vector2Int position);
        public ItemObject GetItemObject(Vector2Int position);
        public OutlineTileMapCellData FormatMainTileMapOutlineData(Vector3Int cellPosition);
        public Vector2Int GetHitTilePosition(Vector2 position);
    }
    

    public interface ITileMapListener {
        void TileUpdate(Vector2Int position);
    }
    
    public abstract class AbstractIWorldTileMap<TItem> : MonoBehaviour, IHitableTileMap, IWorldTileMap where TItem : ItemObject
    {
        protected TileMapType type;
        public TileMapType Type => type;
        protected Tilemap tilemap;
        public Tilemap mTileMap {get{return tilemap;}}
        
        protected TilemapCollider2D tilemapCollider;
        protected HashSet<Vector2Int> partitions = new HashSet<Vector2Int>();
        protected ClosedChunkSystem closedChunkSystem;
        private List<ITileMapListener> listeners = new List<ITileMapListener>();
        private float baseZValue;

        public virtual void Initialize(TileMapType type)
        {
            this.type = type;
            tilemap = gameObject.AddComponent<Tilemap>();
            baseZValue = transform.position.z;
            
            gameObject.AddComponent<TilemapRenderer>();
            if (type.hasCollider()) {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
                // why can't we just disable this unity. God forbid some poor soul manages to break this many blocks. RIP PC
                tilemapCollider.maximumTileChangeCount=int.MaxValue; 
            }
            closedChunkSystem = transform.parent.GetComponentInParent<ClosedChunkSystem>();
        }
        
        public virtual void AddPartition(IChunkPartition partition) {
            partitions.Add(partition.GetRealPosition());
        }
        public virtual IEnumerator RemovePartition(Vector2Int partitionPosition) {
            if (!ContainsPartition(partitionPosition)) {
                yield return null;
            }
            partitions.Remove(partitionPosition);
            int partitionX = partitionPosition.x*Global.CHUNK_PARTITION_SIZE;
            int partitionY = partitionPosition.y*Global.CHUNK_PARTITION_SIZE;
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y ++) {
                    RemoveTile(partitionX+x,partitionY+y);
                }
                yield return null;
            }
        }

        public void AddListener(ITileMapListener listener) {
            listeners.Add(listener);
        }
        protected virtual void RemoveTile(int x, int y) {
            tilemap.SetTile(new Vector3Int(x,y,0),null);
        }
        public bool ContainsPartition(Vector2Int partitionPosition) {
            return this.partitions.Contains(partitionPosition);
        }

        /// <summary>
        /// Writes to partition on place
        /// </summary>
        public virtual void PlaceNewTileAtLocation(int x, int y, ItemObject itemObject) {
            if (itemObject is not TItem item) {
                Debug.LogWarning($"Tried to place invalid item in {name}");
                return;
            }
            Vector2Int vect = new Vector2Int(x,y);
            Vector2Int tilePosition = GetTilePositionInPartition(vect);
            IChunkPartition partition = GetPartitionAtPosition(vect);
            WriteTile(partition, tilePosition, item);
            SetTile(x, y, (TItem) item);
            CallListeners(vect);
        }

        public void CallListeners(Vector2Int position) {
            foreach (ITileMapListener listener in listeners) {
                listener.TileUpdate(position);
            }
        }

        /// <summary>
        /// This will call set tile without modifying any meta data around the tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="item"></param>
        public void RefreshTile(int x, int y, ItemObject item)
        {
            SetTile(x, y, item as TItem);
        }
        protected abstract void WriteTile(IChunkPartition partition, Vector2Int positionInPartition, TItem item);
        /// <summary>
        /// Doesn't write to partition on place as is called from partition
        /// </summary>
        public void PlaceItemTileAtLocation(Vector2Int partitionPosition, Vector2Int tilePartitionPosition, ItemObject item)
        {
            Vector2Int cellPosition = partitionPosition*Global.CHUNK_PARTITION_SIZE + tilePartitionPosition;
            SetTile(cellPosition.x, cellPosition.y, (TItem) item);
        }
        public void PlaceTileAtLocation(Vector2Int position, TileBase tileBase) {
            CallListeners(position);
            tilemap.SetTile((Vector3Int) position,tileBase);
        }
        public abstract bool HitTile(Vector2 position, bool dropItem);

        public virtual bool DeleteTile(Vector2 position) {
            Vector2Int hitTilePosition = GetHitTilePosition(position);
            Vector3Int vect = new Vector3Int(hitTilePosition.x, hitTilePosition.y, 0);
            if (!HasTile(vect)) return false;
            BreakTile(hitTilePosition);
            IChunkPartition partition = GetPartitionAtPosition(hitTilePosition);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(hitTilePosition);
            WriteTile(partition,tilePositionInPartition,null);
            CallListeners(hitTilePosition);
            return true;
        }
        
        public virtual bool DeleteTile(Vector2Int cellPosition) {
            return DeleteTile(mTileMap.CellToWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0)));
        }

        public virtual bool HasTile(Vector3Int vector3Int)
        {
            return mTileMap.GetTile(vector3Int);
        }

        protected abstract void SetTile(int x, int y,TItem item);
        public static Vector2Int GetChunkPosition(Vector2Int position) {
            float x = (float) position.x;
            float y = (float) position.y;
            return new Vector2Int(Mathf.FloorToInt(x/(Global.CHUNK_SIZE)), Mathf.FloorToInt(y/(Global.CHUNK_SIZE)));
        }
        protected Vector2Int GetPartitionPosition(Vector2Int position) {
            float x = (float) position.x;
            float y = (float) position.y;
            return new Vector2Int(Mathf.FloorToInt(x/Global.CHUNK_PARTITION_SIZE), Mathf.FloorToInt((y/Global.CHUNK_PARTITION_SIZE)));
        }
        protected Vector2Int GetTilePositionInPartition(Vector2Int position) {
            return new Vector2Int(Global.modInt(position.x,Global.CHUNK_PARTITION_SIZE),Global.modInt(position.y,Global.CHUNK_PARTITION_SIZE));
        }

        protected IChunkPartition GetPartitionAtPosition(Vector2Int cellPosition) {
            Vector2Int chunkPosition = Global.getChunkFromCell(cellPosition);
            ILoadedChunk chunk = closedChunkSystem.GetChunk(chunkPosition);
            if (chunk == null) {
                return null;
            }
            Vector2Int partitionPosition = GetPartitionPosition(cellPosition);
            Vector2Int partitionPositionInChunk = partitionPosition - chunkPosition * Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = chunk.GetPartition(partitionPositionInChunk);
            return partition;
        }

        public abstract Vector2Int GetHitTilePosition(Vector2 position);

        public Vector2Int GetHitTilePosition(Vector2Int position)
        {
            Vector2 worldPosition = tilemap.CellToWorld(new Vector3Int(position.x, position.y, 0));
            return GetHitTilePosition(worldPosition);
        }
        public Vector2Int WorldToTileMapPosition(Vector2 position) {
            Vector3Int vect = tilemap.WorldToCell(position);
            return new Vector2Int(vect.x,vect.y);
        }
        public virtual void BreakTile(Vector2Int position) {
            Vector3Int vector3Int = new Vector3Int(position.x, position.y, 0);
            tilemap.SetTile(vector3Int, null);
        }

        public abstract ItemObject GetItemObject(Vector2Int position);
        public OutlineTileMapCellData FormatMainTileMapOutlineData(Vector3Int cellPosition)
        {
            return new OutlineTileMapCellData(tilemap.GetTile(cellPosition), null,tilemap.GetTransformMatrix(cellPosition).rotation,tilemap.GetTransformMatrix(cellPosition).rotation);
        }

        public abstract bool BreakAndDropTile(Vector2Int position, bool dropItem);

        public ClosedChunkSystem GetSystem()
        {
            return closedChunkSystem;
        }

        protected virtual void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);
            if (chunk == null) {
                return;
            }
            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            ItemSlot itemSlot = ItemSlotFactory.CreateNewItemSlot(itemObject,amount);
            ItemEntityFactory.SpawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.GetEntityContainer());
        }

        protected ILoadedChunk GetChunk(Vector2Int hitTilePosition) {
            Vector2Int chunkPosition = GetChunkPosition(hitTilePosition);
            
            return closedChunkSystem.GetChunk(chunkPosition);
        }
    
        public TileMapType GetTileMapType()
        {
            return type;
        }

        public bool HasTile(Vector2Int position)
        {
            return mTileMap.GetTile(new Vector3Int(position.x, position.y, 0));
        }
        /// <summary>
        /// Removes the tile from the tilemap without modifying any data
        /// </summary>
        public void RemoveForSwitch(Vector2Int position) {
            tilemap.SetTile((Vector3Int)position,null);
        }

        /// <summary>
        /// Brings this tilemap to the foreground
        /// </summary>
        public void SetHighlight(bool on)
        {
            if (on) {
                Vector3 position = transform.position;
                position.z = 1;
                transform.position = position;
            } else {
                Vector3 position = transform.position;
                position.z = baseZValue;
                transform.position = position;
            }
        }

        public Tilemap GetTilemap()
        {
            return tilemap;
        }
    }
}



















