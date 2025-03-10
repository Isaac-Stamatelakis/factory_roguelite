using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Chunks;
using TileEntity;
using TileMaps.Layer;
using TileMaps.Type;
using TileMaps.Place;
using Chunks.Partitions;
using Chunks.Systems;
using Tiles;
using Items;
using Entities;
using Item.ItemObjects.Instances.Tile.Chisel;
using Item.Slot;
using Player;
using Robot.Tool.Instances;
using Robot.Upgrades;
using TileEntity.MultiBlock;

namespace TileMaps {
    public interface ITileGridMap {
        public TileItem getTileItem(Vector2Int cellPosition);
    }

    public interface IChiselableTileMap
    {
        public void IterateChiselTile(Vector2Int position, int direction);
    }

    public interface IRotatableTileMap
    {
        public void IterateRotatableTile(Vector2Int position, int direction, BaseTileData baseTileData);
    }

    public interface IHammerTileMap
    {
        public void IterateHammerTile(Vector2Int position, int direction);
    }
    public class WorldTileGridMap : AbstractIWorldTileMap<TileItem>, ITileGridMap, IChiselableTileMap, IRotatableTileMap, IHammerTileMap, IConditionalHitableTileMap
    {
        private Tilemap overlayTileMap;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            GameObject overlayTileMapObject = new GameObject("OverlayTileMap");
            overlayTileMapObject.transform.SetParent(transform,false);
            overlayTileMap = overlayTileMapObject.AddComponent<Tilemap>();
            overlayTileMapObject.AddComponent<TilemapRenderer>();
            overlayTileMapObject.transform.localPosition = new Vector3(0, 0, -0.1f);

        }

        protected override void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            SpawnItemEntity(new ItemSlot(itemObject,amount,null), hitTilePosition);
        }
        
        protected void SpawnItemEntity(ItemSlot itemSlot, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);  

            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            Sprite[] itemSprites = itemSlot.itemObject.getSprites();
            if (itemSprites.Length == 0) {
                Debug.LogError("Tried to spawn item with no sprite");
                return;
            }
            Vector2 spriteSize =  Global.getSpriteSize(itemSprites[0]);
            if (PlaceTile.mod(spriteSize.x,2) == 0) {
                realXPosition += 0.25f;
            }
            if (PlaceTile.mod(spriteSize.y,2) == 0) {
                realYPosition += 0.25f;
            }
            ItemEntityFactory.SpawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }
        
        protected override Vector2Int GetHitTilePosition(Vector2 position)
        {
            Vector2Int hitPosition = worldToTileMapPosition(position);
            int maxSearchWidth = 16;
            int searchWidth = 1;
            while (searchWidth < maxSearchWidth) {
                if (Global.modInt(searchWidth,2) == 0) {
                    for (int x = searchWidth/2-1; x >= -searchWidth/2; x --) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y-(searchWidth/2),0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+x,hitPosition.y-(searchWidth/2));
                        }
                    }
                    for (int y = -searchWidth/2+1; y <= searchWidth/2-1; y ++) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x-(searchWidth/2),hitPosition.y+y,0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x-(searchWidth/2), hitPosition.y+y);
                        }
                    }
                } else {
                    for (int x = -(searchWidth-1)/2; x <= (searchWidth-1)/2; x ++) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2,0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2);
                        }
                    }
                    for (int y = (searchWidth-1)/2-1; y >= -(searchWidth-1)/2; y --) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+(searchWidth-1)/2,hitPosition.y+y,0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+(searchWidth-1)/2, hitPosition.y+y);
                        }
                    }
                }
                searchWidth ++;
            }
            return new Vector2Int(-2147483647,-2147483647);
        }
        private bool isHitTile(TileBase tileBase, int searchWidth) {
            int spriteY = 0;
            if (tileBase is Tile) {
                spriteY = (int) Global.getSpriteSize(((Tile) tileBase).sprite).y;
            } else if (tileBase is RuleTile) {
                spriteY = (int) Global.getSpriteSize(((RuleTile) tileBase).m_DefaultSprite).y;
            } else if (tileBase is AnimatedTile) {
                spriteY = (int) Global.getSpriteSize(((AnimatedTile) tileBase).m_AnimatedSprites[0]).y;
            }
            return spriteY >= searchWidth;
        } 
        public override void BreakTile(Vector2Int position) {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) {
                return;
            }
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            ITileEntityInstance tileEntity = GetTileEntityAtPosition(position);
            if (tileEntity != null) {
                TileMapLayer layer = type.toLayer();
                partition.BreakTileEntity(layer,tilePositionInPartition);
                DeleteTileEntityFromConduit(position);
            }
            Vector3Int vector = new Vector3Int(position.x,position.y,0);
            tilemap.SetTile(vector, null);
            if (overlayTileMap.GetTile(vector)) overlayTileMap.SetTile(vector, null);
            
            WriteTile(partition,tilePositionInPartition,null);
            TileHelper.tilePlaceTileEntityUpdate(position, null,this);
            CallListeners(position);
            if (tileEntity is IMultiBlockTileEntity multiBlockTileEntity)
            {
                List<IMultiBlockTileAggregate> aggregates = TileEntityUtils.BFSTileEntityComponent<IMultiBlockTileAggregate>(tileEntity,TileType.Block);
                foreach (IMultiBlockTileAggregate aggregate in aggregates)
                {
                    if (!ReferenceEquals(aggregate.GetAggregator(),multiBlockTileEntity)) continue;
                    aggregate.SetAggregator(null);
                }
            }
            if (tileEntity is IMultiBlockTileAggregate multiBlockTileAggregate)
            {
                IMultiBlockTileEntity aggregator = multiBlockTileAggregate.GetAggregator();
                if (aggregator == null) return;
                TileEntityUtils.RefreshMultiBlock(aggregator);
            }
        }

        public override ItemObject GetItemObject(Vector2Int position)
        {
            return getTileItem(position);
        }

        public override bool BreakAndDropTile(Vector2Int position, bool dropItem)
        {
            if (!mTileMap.GetTile(new Vector3Int(position.x,position.y,0))) return false;
            
            if (dropItem) 
            {
                TileItem tileItem = getTileItem(position);
                DropItem(tileItem, position);
            }
            BreakTile(position);
            return true;
        }

        public ITileEntityInstance GetTileEntityAtPosition(Vector2Int position) {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) {
                return null;
            }
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            return partition.GetTileEntity(tilePositionInPartition);
        }
        protected void DeleteTileEntityFromConduit(Vector2Int position) {
            if (base.closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                conduitTileClosedChunkSystem.TileEntityDeleteUpdate(position);
            }
        }

        protected bool HitHardness(Vector2Int cellPosition) {
            TileItem tileItem = getTileItem(cellPosition);
            if (ReferenceEquals(tileItem, null) || !tileItem.tileOptions.hitable) return false;
        
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(cellPosition);
            
            bool broken = partition.DeIncrementHardness(tilePositionInPartition);
            if (tileItem.tileType.toTileMapType().toLayer() != TileMapLayer.Base || !CanShowBreakIndiciator(tileItem.tile, partition, tilePositionInPartition)) return broken;
            
            if (!broken) {
                int hardness = partition.GetHardness(tilePositionInPartition);
                float breakRatio = 1f - ((float)hardness) / tileItem.tileOptions.hardness;
                closedChunkSystem.BreakIndicator.setBreak(breakRatio,cellPosition);
            } else {
                closedChunkSystem.BreakIndicator.removeBreak(cellPosition);
            }
            return broken;
        }

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            Vector3Int vector = new Vector3Int(x,y,0);
            if (overlayTileMap.GetTile(vector)) overlayTileMap.SetTile(vector, null);
        }

        private bool CanShowBreakIndiciator(TileBase tileBase, IChunkPartition partition, Vector2Int positionInPartition)
        {
            switch (tileBase)
            {
                case Tile tile:
                    return tile.colliderType == Tile.ColliderType.Grid;
                case AnimatedTile animatedTile:
                    return animatedTile.m_TileColliderType == Tile.ColliderType.Grid;
                case IStateRotationTile stateRotationTile:
                {
                    BaseTileData tileData = partition.GetBaseData(positionInPartition);
                    TileBase rotTile = stateRotationTile.getTile(tileData.rotation,tileData.mirror);
                    return CanShowBreakIndiciator(rotTile, partition, positionInPartition);
                }
                case IStateTile stateTile:
                {
                    BaseTileData tileData = partition.GetBaseData(positionInPartition);
                    TileBase hammerTileState = stateTile.getTileAtState(tileData.state);
                    return CanShowBreakIndiciator(hammerTileState, partition, positionInPartition);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileBase));
            }
        }

        protected override void SetTile(int x, int y,TileItem tileItem) {
            TileBase tileBase = tileItem.tile;
            if (ReferenceEquals(tileBase,null)) return;
            
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            
            if (tileBase is IStateTile stateTile) {
                tileBase = stateTile.getTileAtState(baseTileData.state);
            } 
            Vector3Int vector3Int = new Vector3Int(position.x,position.y,0);
            bool rotatable = tileItem.tileOptions.rotatable;
            TileBase overlayTile = tileItem.tileOptions.Overlay.Tile;
            if (overlayTile)
            {
                if (rotatable)
                {
                    PlaceTile.RotateTileInMap(overlayTileMap, overlayTile, vector3Int, baseTileData.rotation,baseTileData.mirror);
                }
                else
                {
                    overlayTileMap.SetTile(vector3Int, overlayTile);
                }
                overlayTileMap.SetTileFlags(vector3Int, TileFlags.None); // Required to get color to work
                overlayTileMap.SetColor(vector3Int,tileItem.tileOptions.Overlay.Color);
            }
            if (!rotatable) 
            {
                tilemap.SetTile(vector3Int,tileBase);
                return;
            }
            
            if (tileBase is IStateRotationTile stateRotationTile) {
                tilemap.SetTile(
                    vector3Int, 
                    stateRotationTile.getTile(baseTileData.rotation,baseTileData.mirror)
                );
                return;
            }
            PlaceTile.RotateTileInMap(tilemap, tileBase, vector3Int, baseTileData.rotation,baseTileData.mirror);
        }
        
        
        
        public override bool HitTile(Vector2 position, bool dropItem) {
            Vector2Int hitTilePosition = GetHitTilePosition(position);
            if (!HitHardness(hitTilePosition)) return false;
            return BreakAndDropTile(hitTilePosition, dropItem);
        }

        public bool CanHitTile(int power, Vector2 position)
        {
            Vector2Int hitTilePosition = GetHitTilePosition(position);
            TileItem tileItem = getTileItem(hitTilePosition);
            if (ReferenceEquals(tileItem, null)) return false;
            return (int)tileItem.tileOptions.requiredToolTier <= power;
        }

        private void DropItem(TileItem tileItem, Vector2Int hitTilePosition)
        {
            List<ItemSlot> dropItems = ItemSlotUtils.GetTileItemDrop(tileItem);
            foreach (ItemSlot itemSlot in dropItems)
            {
                SpawnItemEntity(itemSlot,hitTilePosition);
            }
        }

        protected override void WriteTile(IChunkPartition partition, Vector2Int positionInPartition, TileItem item)
        {
            partition?.SetTile(positionInPartition,getType().toLayer(),item);
        }

        public TileItem getTileItem(Vector2Int cellPosition) {
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            if (partition == null) {
                return null;
            }
            Vector2Int positionInPartition = GetTilePositionInPartition(cellPosition);
            TileItem tileItem = partition.GetTileItem(positionInPartition,getType().toLayer());
            return tileItem;
        }
        
        public TileItem GetTileItem(Vector2 worldPosition)
        {
            Vector2Int cellPosition = (Vector2Int)tilemap.WorldToCell(worldPosition);
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            if (partition == null) {
                return null;
            }
            Vector2Int positionInPartition = GetTilePositionInPartition(cellPosition);
            TileItem tileItem = partition.GetTileItem(positionInPartition,getType().toLayer());
            return tileItem;
        }
        
        public void IterateChiselTile(Vector2Int position, int direction)
        {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return;
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            
            TileItem tileItem = getTileItem(position);
            if (ReferenceEquals(tileItem, null)) return;
            
            if (tileItem is not ChiselTileItem chiselTileItem) return;
            
            ChiselTileItem newChiselTileItem = ChiselItemUtils.Iterate(direction, chiselTileItem);
            SetTile(position.x,position.y,newChiselTileItem);
            WriteTile(partition,tilePositionInPartition,newChiselTileItem);
        }

        public void IterateRotatableTile(Vector2Int position, int direction, BaseTileData baseTileData)
        {
            TileItem tileItem = getTileItem(position);
            int newRotation = Buildinator.CalculateNewRotation(baseTileData.rotation, direction);
           
            Vector2 worldPosition = tilemap.CellToWorld((Vector3Int)position);
            
            FloatIntervalVector exclusion = TileHelper.getRealCoveredArea(worldPosition, Global.getSpriteSize(tileItem.getSprite()), baseTileData.rotation);
            if (!PlaceTile.BaseTilePlacable(tileItem, worldPosition, closedChunkSystem, newRotation, exclusion))
            {
                return;
            }
            
            var (partition, positionInPartition) = ((IChunkSystem)closedChunkSystem).GetPartitionAndPositionAtCellPosition(position);

            ITileEntityInstance tileEntityInstance =  partition.GetTileEntity(positionInPartition);
            IConduitPortTileEntity portTileEntity = tileEntityInstance as IConduitPortTileEntity;
            ConduitTileClosedChunkSystem conduitTileClosedChunkSystem = closedChunkSystem as ConduitTileClosedChunkSystem;
            bool updatePort = portTileEntity != null && !ReferenceEquals(conduitTileClosedChunkSystem, null);
            
            // Important to delete the tile entity before the rotation changes
            if (updatePort) conduitTileClosedChunkSystem.TileEntityDeleteUpdate(position);
            
            baseTileData.rotation = newRotation;
            SetTile(position.x,position.y,tileItem);
            if (updatePort) conduitTileClosedChunkSystem.TileEntityPlaceUpdate(tileEntityInstance);  
        }

        public void IterateHammerTile(Vector2Int position, int direction)
        {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return;
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            
            TileItem tileItem = getTileItem(position);
            if (ReferenceEquals(tileItem, null)) return;
            if (tileItem.tile is not HammerTile hammerTile) return;
            
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            int stateCount = hammerTile.getStateAmount();
            int newState = ((baseTileData.state+direction) % stateCount + stateCount) % stateCount;
            baseTileData.state = newState;
 
            SetTile(position.x,position.y,tileItem);
        }
    }
}
















