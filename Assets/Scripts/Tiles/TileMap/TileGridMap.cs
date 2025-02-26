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
        protected override void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);  

            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            Sprite[] itemSprites = itemObject.getSprites();
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
            ItemSlot itemSlot = ItemSlotFactory.CreateNewItemSlot(itemObject,amount);
            ItemEntityFactory.SpawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }

        public TileOptions getOptionsAtPosition(Vector2Int realTilePosition) {
            if (realTilePosition == new Vector2Int(-2147483647,-2147483647)) {
                return null;
            }
            IChunkPartition partition = GetPartitionAtPosition(realTilePosition);
            Vector2Int tilePositionInPartition = base.GetTilePositionInPartition(realTilePosition);
            return partition.GetTileItem(tilePositionInPartition,TileMapLayer.Base).tileOptions;
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
            ITileEntityInstance tileEntity = getTileEntityAtPosition(position);
            if (tileEntity != null) {
                TileMapLayer layer = type.toLayer();
                partition.BreakTileEntity(layer,tilePositionInPartition);
                deleteTileEntityFromConduit(position);
            }
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
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

        public ITileEntityInstance getTileEntityAtPosition(Vector2Int position) {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) {
                return null;
            }
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            return partition.GetTileEntity(tilePositionInPartition);
        }
        protected void deleteTileEntityFromConduit(Vector2Int position) {
            if (base.closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                conduitTileClosedChunkSystem.TileEntityDeleteUpdate(position);
            }
        }

        protected bool hitHardness(Vector2Int cellPosition) {
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
            if (!tileItem.tileOptions.rotatable) 
            {
                tilemap.SetTile(new Vector3Int(x,y,0),tileBase);
                return;
            }
            
            if (tileBase is IStateRotationTile stateRotationTile) {
                tilemap.SetTile(
                    new Vector3Int(x,y,0), 
                    stateRotationTile.getTile(baseTileData.rotation,baseTileData.mirror)
                );
                return;
            }
            PlaceTile.RotateTileInMap(tilemap, tileBase, new Vector3Int(x, y, 0), baseTileData.rotation,baseTileData.mirror);
        }
        
        
        public override void HitTile(Vector2 position) {
            Vector2Int hitTilePosition = GetHitTilePosition(position);
            if (!hitHardness(hitTilePosition)) return;
            
            TileItem tileItem = getTileItem(hitTilePosition);
            DropItem(tileItem, hitTilePosition);
            
            BreakTile(hitTilePosition);
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
            var dropOptions = tileItem.tileOptions.dropOptions;
            if (dropOptions.Count == 0) {
                SpawnItemEntity(tileItem,1,hitTilePosition);
                return;
            }

            if (dropOptions.Count == 1) // Optimization for common case
            {
                DropOption dropOption = dropOptions[0];
                if (dropOption.lowerAmount == dropOption.upperAmount)
                {
                    SpawnItemEntity(dropOption.itemObject,(uint)dropOption.lowerAmount,hitTilePosition);
                    return;
                }
            }
            int totalWeight = 0;
            foreach (DropOption dropOption in dropOptions) {
                totalWeight += dropOption.weight;
            }
            
            int ran = UnityEngine.Random.Range(0,totalWeight);
            totalWeight = 0;
            foreach (DropOption dropOption in dropOptions) {
                totalWeight += dropOption.weight;
                if (totalWeight < ran) continue;
                if (ReferenceEquals(dropOption.itemObject, null)) continue;
                
                uint amount = (uint)UnityEngine.Random.Range(dropOption.lowerAmount,dropOption.upperAmount+1);
                amount = GlobalHelper.MaxUInt(1, amount);
                SpawnItemEntity(dropOption.itemObject,amount,hitTilePosition);
                return;
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
        
        public TileItem getTileItem(Vector2 worldPosition)
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
















