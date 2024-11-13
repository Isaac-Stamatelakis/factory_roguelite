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
using Items;

namespace TileMaps {
    public interface IHitableTileMap {
        public void hitTile(Vector2 position);
        public void deleteTile(Vector2 position);
    }

    public interface ITileMap {
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
    }

    public interface ITileMapListener {
        void tileUpdate(Vector2Int position);
    }
    
    public abstract class AbstractTileMap<Item> : MonoBehaviour, IHitableTileMap, ITileMap where Item : ItemObject
    {
        public TileMapType type;
        protected Tilemap tilemap;
        public Tilemap mTileMap {get{return tilemap;}}
        protected TilemapRenderer tilemapRenderer;
        protected TilemapCollider2D tilemapCollider;
        protected HashSet<Vector2Int> partitions;
        protected ClosedChunkSystem closedChunkSystem;
        private List<ITileMapListener> listeners = new List<ITileMapListener>();
        private float baseZValue;

        public virtual void Start() {
            tilemap = gameObject.AddComponent<Tilemap>();
            baseZValue = transform.position.z;
            partitions = new HashSet<Vector2Int>();
            tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
            if (type.hasCollider()) {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
                // why can't we just disable this unity. God forbid some poor soul manages to break this many blocks. RIP PC
                tilemapCollider.maximumTileChangeCount=int.MaxValue; 
            }
            closedChunkSystem = transform.parent.GetComponentInParent<ClosedChunkSystem>();
        }
        
        public virtual void addPartition(IChunkPartition partition) {
            partitions.Add(partition.getRealPosition());
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
                    removeTile(partitionX+x,partitionY+y);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public void addListener(ITileMapListener listener) {
            listeners.Add(listener);
        }
        protected virtual void removeTile(int x, int y) {
            tilemap.SetTile(new Vector3Int(x,y,0),null);
        }
        public bool containsPartition(Vector2Int partitionPosition) {
            return this.partitions.Contains(partitionPosition);
        }

        /// <summary>
        /// Writes to partition on place
        /// </summary>
        public virtual void placeNewTileAtLocation(int x, int y, ItemObject itemObject) {
            if (itemObject is not Item item) {
                Debug.LogWarning($"Tried to place invalid item in {name}");
                return;
            }
            Vector2Int vect = new Vector2Int(x,y);
            Vector2Int tilePosition = getTilePositionInPartition(vect);
            IChunkPartition partition = getPartitionAtPosition(vect);
            writeTile(partition, tilePosition, item);
            setTile(x, y, (Item) item);
        }

        public void callListeners(Vector2Int position) {
            foreach (ITileMapListener listener in listeners) {
                listener.tileUpdate(position);
            }
        }
        protected abstract void writeTile(IChunkPartition partition, Vector2Int position, Item item);
        /// <summary>
        /// Doesn't write to partition on place as is called from partition
        /// </summary>
        public void placeItemTileAtLocation(Vector2Int partitionPosition, Vector2Int tilePartitionPosition, ItemObject item)
        {
            Vector2Int cellPosition = partitionPosition*Global.ChunkPartitionSize + tilePartitionPosition;
            callListeners(cellPosition);
            setTile(cellPosition.x, cellPosition.y, (Item) item);
        }
        public void placeTileAtLocation(Vector2Int position, TileBase tileBase) {
            callListeners(position);
            tilemap.SetTile((Vector3Int) position,tileBase);
        }
        public abstract void hitTile(Vector2 position);

        public virtual void deleteTile(Vector2 position) {
            Vector2Int hitTilePosition = getHitTilePosition(position);
            breakTile(hitTilePosition);
            IChunkPartition partition = getPartitionAtPosition(hitTilePosition);
            Vector2Int tilePositionInPartition = getTilePositionInPartition(hitTilePosition);
            writeTile(partition,tilePositionInPartition,null);
            callListeners(hitTilePosition);
        }

        protected abstract void setTile(int x, int y,Item item);
        protected Vector2Int getChunkPosition(Vector2Int position) {
            float x = (float) position.x;
            float y = (float) position.y;
            return new Vector2Int(Mathf.FloorToInt(x/(Global.ChunkSize)), Mathf.FloorToInt(y/(Global.ChunkSize)));
        }
        protected Vector2Int getPartitionPosition(Vector2Int position) {
            float x = (float) position.x;
            float y = (float) position.y;
            return new Vector2Int(Mathf.FloorToInt(x/Global.ChunkPartitionSize), Mathf.FloorToInt((y/Global.ChunkPartitionSize)));
        }
        protected Vector2Int getTilePositionInPartition(Vector2Int position) {
            return new Vector2Int(Global.modInt(position.x,Global.ChunkPartitionSize),Global.modInt(position.y,Global.ChunkPartitionSize));
        }

        protected IChunkPartition getPartitionAtPosition(Vector2Int cellPosition) {
            Vector2Int chunkPosition = Global.getChunkFromCell(cellPosition);
            ILoadedChunk chunk = closedChunkSystem.getChunk(chunkPosition);
            if (chunk == null) {
                return null;
            }
            Vector2Int partitionPosition = getPartitionPosition(cellPosition);
            Vector2Int partitionPositionInChunk = partitionPosition - chunkPosition * Global.PartitionsPerChunk;
            IChunkPartition partition = chunk.getPartition(partitionPositionInChunk);
            return partition;
        }

        protected abstract Vector2Int getHitTilePosition(Vector2 position);

        public Vector2Int worldToTileMapPosition(Vector2 position) {
            Vector3Int vect = tilemap.WorldToCell(position);
            return new Vector2Int(vect.x,vect.y);
        }
        protected virtual void breakTile(Vector2Int position) {
            Vector2Int chunkPartition = getPartitionPosition(position);
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
            Vector2Int tilePositon = getTilePositionInPartition(position);
        }
        protected virtual void spawnItemEntity(ItemObject itemObject, int amount, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = getChunk(hitTilePosition);
            if (chunk == null) {
                return;
            }
            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            ItemSlot itemSlot = ItemSlotFactory.createNewItemSlot(itemObject,amount);
            ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }

        protected ILoadedChunk getChunk(Vector2Int hitTilePosition) {
            Vector2Int chunkPosition = getChunkPosition(hitTilePosition);
            
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
    }
}



















