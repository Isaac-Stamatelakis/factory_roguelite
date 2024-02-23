using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using ChunkModule;
using TileMapModule.Type;
using ChunkModule.ClosedChunkSystemModule;
using TileMapModule.Place;
using Tiles;
using ChunkModule.PartitionModule;

namespace TileMapModule {
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
    }
    /**
    Takes in a 16 x 16 array of tileIDs and creates a TileMap out of them
    **/

    public abstract class AbstractTileMap<Item> : MonoBehaviour, IHitableTileMap, ITileMap where Item : ItemObject
    {
        public TileMapType type;
        protected Tilemap tilemap;
        public Tilemap mTileMap {get{return tilemap;}}
        protected TilemapRenderer tilemapRenderer;
        protected TilemapCollider2D tilemapCollider;
        protected HashSet<Vector2Int> partitions;
        protected DevMode devMode;
        protected ClosedChunkSystem closedChunkSystem;


        public virtual void Start() {
            tilemap = gameObject.AddComponent<Tilemap>();
            partitions = new HashSet<Vector2Int>();
            tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
            if (type.hasCollider()) {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
                // why can't we just disable this unity. God forbid some poor soul manages to break this many blocks. RIP PC
                tilemapCollider.maximumTileChangeCount=100000000; 
            }
            closedChunkSystem = transform.parent.GetComponent<ClosedChunkSystem>();
            devMode = GameObject.Find("Player").GetComponent<DevMode>();
            
        }
        
        public void addPartition(IChunkPartition partition) {
            partitions.Add(partition.getRealPosition());
        }
        public IEnumerator removePartition(Vector2Int partitionPosition) {
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

        protected void removeTile(int x, int y) {
            tilemap.SetTile(new Vector3Int(x,y,0),null);
        }
        public bool containsPartition(Vector2Int partitionPosition) {
            return this.partitions.Contains(partitionPosition);
        }

        /// <summary>
        /// Writes to partition on place
        /// </summary>
        public virtual void placeNewTileAtLocation(int x, int y, ItemObject itemObject) {
            Vector2Int vect = new Vector2Int(x,y);
            Vector2Int tilePosition = getTilePositionInPartition(vect);
            IChunkPartition partition = getPartitionAtPosition(vect);
            Item item = (Item) itemObject;
            writeTile(partition, tilePosition, item);
            setTile(x, y, (Item) item);
        }

        protected abstract void writeTile(IChunkPartition partition, Vector2Int position, Item item);
        /// <summary>
        /// Doesn't write to partition on place as is called from partition
        /// </summary>
        public void placeItemTileAtLocation(Vector2Int partitionPosition, Vector2Int tilePartitionPosition, ItemObject item)
        {
            setTile(
                partitionPosition.x *Global.ChunkPartitionSize + tilePartitionPosition.x, 
                partitionPosition.y *Global.ChunkPartitionSize + tilePartitionPosition.y, 
                (Item) item
            );
        }
        public void placeTileAtLocation(Vector2Int position, TileBase tileBase) {
            tilemap.SetTile((Vector3Int) position,tileBase);
        }
        public abstract void hitTile(Vector2 position);

        public virtual void deleteTile(Vector2 position) {
            Vector2Int hitTilePosition = getHitTilePosition(position);
            breakTile(hitTilePosition);
            IChunkPartition partition = getPartitionAtPosition(hitTilePosition);
            Vector2Int tilePositionInPartition = getTilePositionInPartition(hitTilePosition);
            writeTile(partition,tilePositionInPartition,null);
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
            IChunk chunk = closedChunkSystem.getChunk(chunkPosition);
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
        protected virtual bool hitHardness(TileOptions tileOptions) {
            return false;
        }
        protected virtual void breakTile(Vector2Int position) {
            Vector2Int chunkPartition = getPartitionPosition(position);
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
            Vector2Int tilePositon = getTilePositionInPartition(position);
        }
        protected virtual void spawnItemEntity(Item item, Vector2Int hitTilePosition) {
            IChunk chunk = getChunk(hitTilePosition);
            if (chunk == null) {
                return;
            }
            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            ItemSlot itemSlot = new ItemSlot(itemObject: item, amount: 1, nbt : new Dictionary<string, object>());
            ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }

        protected IChunk getChunk(Vector2Int hitTilePosition) {
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
    }
}



















