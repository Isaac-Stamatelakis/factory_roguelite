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
    public interface IHitableTileMap {
        public void hitTile(Vector2 position);
        public void deleteTile(Vector2 position);
    }

    public interface IWorldTileMap {
        public void addPartition(IChunkPartition partition);
        public IEnumerator removePartition(Vector2Int partitionPosition);
        public bool containsPartition(Vector2Int partitionPosition);
        public void placeNewTileAtLocation(int x, int y, ItemObject itemObject);
        public void placeItemTileAtLocation(Vector2Int partition, Vector2Int partitionPosition, ItemObject itemObject);
        public TileMapType getType();
        public Vector2Int worldToTileMapPosition(Vector2 worldPosition);
        public bool hasTile(Vector2Int position);
        public void removeForSwitch(Vector2Int position);
        public void placeTileAtLocation(Vector2Int position, TileBase tileBase);
        public void addListener(ITileMapListener listener);
        public void setHighlight(bool on);
        public Tilemap GetTilemap();
        public void Initialize(TileMapType type);
    }

    public interface ITileMapListener {
        void tileUpdate(Vector2Int position);
    }
    
    public abstract class AbstractIWorldTileMap<TItem> : MonoBehaviour, IHitableTileMap, IWorldTileMap where TItem : ItemObject
    {
        protected TileMapType type;
        public TileMapType Type => type;
        protected Tilemap tilemap;
        public Tilemap mTileMap {get{return tilemap;}}
        protected TilemapRenderer tilemapRenderer;
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
            
            tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
            if (type.hasCollider()) {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
                // why can't we just disable this unity. God forbid some poor soul manages to break this many blocks. RIP PC
                tilemapCollider.maximumTileChangeCount=int.MaxValue; 
            }
            closedChunkSystem = transform.parent.GetComponentInParent<ClosedChunkSystem>();
        }
        
        public virtual void addPartition(IChunkPartition partition) {
            partitions.Add(partition.GetRealPosition());
        }
        public virtual IEnumerator removePartition(Vector2Int partitionPosition) {
            if (!containsPartition(partitionPosition)) {
                yield return null;
            }
            partitions.Remove(partitionPosition);
            int partitionX = partitionPosition.x*Global.ChunkPartitionSize;
            int partitionY = partitionPosition.y*Global.ChunkPartitionSize;
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    RemoveTile(partitionX+x,partitionY+y);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public void addListener(ITileMapListener listener) {
            listeners.Add(listener);
        }
        protected virtual void RemoveTile(int x, int y) {
            tilemap.SetTile(new Vector3Int(x,y,0),null);
        }
        public bool containsPartition(Vector2Int partitionPosition) {
            return this.partitions.Contains(partitionPosition);
        }

        /// <summary>
        /// Writes to partition on place
        /// </summary>
        public virtual void placeNewTileAtLocation(int x, int y, ItemObject itemObject) {
            if (itemObject is not TItem item) {
                Debug.LogWarning($"Tried to place invalid item in {name}");
                return;
            }
            Vector2Int vect = new Vector2Int(x,y);
            Vector2Int tilePosition = GetTilePositionInPartition(vect);
            IChunkPartition partition = GetPartitionAtPosition(vect);
            WriteTile(partition, tilePosition, item);
            SetTile(x, y, (TItem) item);
        }

        public void CallListeners(Vector2Int position) {
            foreach (ITileMapListener listener in listeners) {
                listener.tileUpdate(position);
            }
        }
        protected abstract void WriteTile(IChunkPartition partition, Vector2Int position, TItem item);
        /// <summary>
        /// Doesn't write to partition on place as is called from partition
        /// </summary>
        public void placeItemTileAtLocation(Vector2Int partitionPosition, Vector2Int tilePartitionPosition, ItemObject item)
        {
            Vector2Int cellPosition = partitionPosition*Global.ChunkPartitionSize + tilePartitionPosition;
            CallListeners(cellPosition);
            SetTile(cellPosition.x, cellPosition.y, (TItem) item);
        }
        public void placeTileAtLocation(Vector2Int position, TileBase tileBase) {
            CallListeners(position);
            tilemap.SetTile((Vector3Int) position,tileBase);
        }
        public abstract void hitTile(Vector2 position);

        public virtual void deleteTile(Vector2 position) {
            Vector2Int hitTilePosition = GetHitTilePosition(position);
            BreakTile(hitTilePosition);
            IChunkPartition partition = GetPartitionAtPosition(hitTilePosition);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(hitTilePosition);
            WriteTile(partition,tilePositionInPartition,null);
            CallListeners(hitTilePosition);
        }

        protected abstract void SetTile(int x, int y,TItem item);
        public static Vector2Int GetChunkPosition(Vector2Int position) {
            float x = (float) position.x;
            float y = (float) position.y;
            return new Vector2Int(Mathf.FloorToInt(x/(Global.ChunkSize)), Mathf.FloorToInt(y/(Global.ChunkSize)));
        }
        protected Vector2Int GetPartitionPosition(Vector2Int position) {
            float x = (float) position.x;
            float y = (float) position.y;
            return new Vector2Int(Mathf.FloorToInt(x/Global.ChunkPartitionSize), Mathf.FloorToInt((y/Global.ChunkPartitionSize)));
        }
        protected Vector2Int GetTilePositionInPartition(Vector2Int position) {
            return new Vector2Int(Global.modInt(position.x,Global.ChunkPartitionSize),Global.modInt(position.y,Global.ChunkPartitionSize));
        }

        protected IChunkPartition GetPartitionAtPosition(Vector2Int cellPosition) {
            Vector2Int chunkPosition = Global.getChunkFromCell(cellPosition);
            ILoadedChunk chunk = closedChunkSystem.getChunk(chunkPosition);
            if (chunk == null) {
                return null;
            }
            Vector2Int partitionPosition = GetPartitionPosition(cellPosition);
            Vector2Int partitionPositionInChunk = partitionPosition - chunkPosition * Global.PartitionsPerChunk;
            IChunkPartition partition = chunk.getPartition(partitionPositionInChunk);
            return partition;
        }

        protected abstract Vector2Int GetHitTilePosition(Vector2 position);

        public Vector2Int worldToTileMapPosition(Vector2 position) {
            Vector3Int vect = tilemap.WorldToCell(position);
            return new Vector2Int(vect.x,vect.y);
        }
        protected virtual void BreakTile(Vector2Int position) {
            Vector2Int chunkPartition = GetPartitionPosition(position);
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
            Vector2Int tilePositon = GetTilePositionInPartition(position);
        }
        protected virtual void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);
            if (chunk == null) {
                return;
            }
            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            ItemSlot itemSlot = ItemSlotFactory.CreateNewItemSlot(itemObject,amount);
            ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }

        protected ILoadedChunk GetChunk(Vector2Int hitTilePosition) {
            Vector2Int chunkPosition = GetChunkPosition(hitTilePosition);
            
            return closedChunkSystem.getChunk(chunkPosition);
        }
    
        public TileMapType getType()
        {
            return type;
        }

        public bool hasTile(Vector2Int position)
        {
            return mTileMap.GetTile(new Vector3Int(position.x,position.y,0)) != null;
        }
        /// <summary>
        /// Removes the tile from the tilemap without modifying any data
        /// </summary>
        public void removeForSwitch(Vector2Int position) {
            tilemap.SetTile((Vector3Int)position,null);
        }

        /// <summary>
        /// Brings this tilemap to the foreground
        /// </summary>
        public void setHighlight(bool on)
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



















