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
using Items.Transmutable;
using Player;
using Robot.Tool.Instances;
using Robot.Upgrades;
using TileEntity.MultiBlock;
using Tiles.Options.Overlay;
using Tiles.TileMap;

namespace TileMaps {
    public interface ITileGridMap {
        public OutlineTileMapCellData GetOutlineCellData(Vector3Int cellPosition);
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
    public class WorldTileMap : BaseWorldTileMap<TileItem>, ITileGridMap, IChiselableTileMap, IRotatableTileMap, IHammerTileMap, IConditionalHitableTileMap, ITileMapListener
    {
        protected PrimaryShaderTilemap primaryShaderTilemap;
        protected Tilemap AddOverlay(float z)
        {
            GameObject overlayTileMapObject = new GameObject("OverlayTileMap");
            overlayTileMapObject.transform.SetParent(transform,false);
            var overlayTileMap = overlayTileMapObject.AddComponent<Tilemap>();
            overlayTileMapObject.AddComponent<TilemapRenderer>();
            overlayTileMapObject.transform.localPosition = new Vector3(0, 0, z);
            return overlayTileMap;
        }

        public const float OVERLAY_Z = -0.1f;

        protected override void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            SpawnItemEntity(new ItemSlot(itemObject,amount,null), hitTilePosition);
        }
        
        protected void SpawnItemEntity(ItemSlot itemSlot, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);

            float realXPosition = transform.position.x + hitTilePosition.x / 2f;
            float realYPosition = transform.position.y + hitTilePosition.y / 2f;
            Sprite[] itemSprites = itemSlot.itemObject.GetSprites();
            if (itemSprites.Length == 0) {
                Debug.LogError("Tried to spawn item with no sprite");
                return;
            }
            Vector2 spriteSize =  Global.GetSpriteSize(itemSprites[0]);
            if (TilePlaceUtils.Mod(spriteSize.x,2) == 0) {
                realXPosition += 0.25f;
            }
            if (TilePlaceUtils.Mod(spriteSize.y,2) == 0) {
                realYPosition += 0.25f;
            }
            ItemEntityFactory.SpawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.GetEntityContainer());
        }
        
        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            Vector2Int hitPosition = WorldToTileMapPosition(position);
            int maxSearchWidth = 16;
            int searchWidth = 1;
            if (!tilemap) return new Vector2Int(-2147483647,-2147483647);
            
            while (searchWidth < maxSearchWidth) {
                if (Global.ModInt(searchWidth,2) == 0) {
                    for (int x = searchWidth/2-1; x >= -searchWidth/2; x --) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y-(searchWidth/2),0));
                        if (IsHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+x,hitPosition.y-(searchWidth/2));
                        }
                    }
                    for (int y = -searchWidth/2+1; y <= searchWidth/2-1; y ++) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x-(searchWidth/2),hitPosition.y+y,0));
                        if (IsHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x-(searchWidth/2), hitPosition.y+y);
                        }
                    }
                } else {
                    for (int x = -(searchWidth-1)/2; x <= (searchWidth-1)/2; x ++) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2,0));
                        if (IsHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2);
                        }
                    }
                    for (int y = (searchWidth-1)/2-1; y >= -(searchWidth-1)/2; y --) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+(searchWidth-1)/2,hitPosition.y+y,0));
                        if (IsHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+(searchWidth-1)/2, hitPosition.y+y);
                        }
                    }
                }
                searchWidth ++;
            }
            return new Vector2Int(-2147483647,-2147483647);
        }
        
        private bool IsHitTile(TileBase tileBase, int searchWidth) {
            int spriteY = 0;
            if (tileBase is Tile) {
                spriteY = (int) Global.GetSpriteSize(((Tile) tileBase).sprite).y;
            } else if (tileBase is RuleTile) {
                spriteY = (int) Global.GetSpriteSize(((RuleTile) tileBase).m_DefaultSprite).y;
            } else if (tileBase is AnimatedTile) {
                spriteY = (int) Global.GetSpriteSize(((AnimatedTile) tileBase).m_AnimatedSprites[0]).y;
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
                TileMapLayer layer = type.ToLayer();
                partition.BreakTileEntity(layer,tilePositionInPartition);
                DeleteTileEntityFromConduit(position);
            }
            Vector3Int vector = new Vector3Int(position.x,position.y,0);
            
            primaryShaderTilemap?.RemoveTile(position);
            RemoveTile(vector.x,vector.y);
            TileItem tileItem = partition.GetTileItem(tilePositionInPartition, TileMapLayer.Base);
            
            WriteTile(partition,tilePositionInPartition,null);
            TileHelper.tilePlaceTileEntityUpdate(position, null,this);
            UpdateListeners(position, tileItem);
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
            Vector3Int vector3Int = new Vector3Int(position.x,position.y,0);
            if (!mTileMap.GetTile(vector3Int)) return false;
            if (dropItem) 
            {
                TileItem tileItem = getTileItem(position);
                DropItem(tileItem, position);
            }
            
            BreakTile(position);
            return true;
        }

        private void UpdateListeners(Vector2Int position, TileItem tileItem)
        {
            if (!tileItem) return;
            Vector2Int spriteSize = Global.GetSpriteSize(tileItem.GetSprite());
            if (spriteSize.x <= 1 && spriteSize.y <= 1)
            {
                CallListeners(position);
            }
            else
            {
                IChunkPartition partition = GetPartitionAtPosition(position);
                if (partition != null)
                {
                    Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
                    BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
                    // Use interval vector because I have infrastructure for it.
                    FloatIntervalVector coveredArea = TileHelper.getRealCoveredArea(mTileMap.CellToWorld(new Vector3Int(position.x,position.y,0)),spriteSize , baseTileData.rotation);
                    
                    // Now have to wrap back to ints
                    Vector3Int min = mTileMap.WorldToCell(new Vector3(coveredArea.X.LowerBound,coveredArea.Y.LowerBound));
                    Vector3Int max =  mTileMap.WorldToCell(new Vector3(coveredArea.X.UpperBound,coveredArea.Y.UpperBound));
                    for (int x = min.x; x <= max.x; x++)
                    {
                        for (int y = min.y; y <= max.y; y++)
                        {
                            CallListeners(new Vector2Int(x,y));
                        }
                    }
                }
            }
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
            if (base.closedChunkSystem is ConduitClosedChunkSystem conduitTileClosedChunkSystem) {
                conduitTileClosedChunkSystem.TileEntityDeleteUpdate(position);
            }
        }

        protected bool HitHardness(Vector2Int cellPosition) {
            TileItem tileItem = getTileItem(cellPosition);
            if (ReferenceEquals(tileItem, null) || !tileItem.tileOptions.hitable) return false;
        
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(cellPosition);
            
            bool broken = partition.DeIncrementHardness(tilePositionInPartition);
            if (tileItem.tileType != TileType.Block) return broken;
            
            if (!broken) {
                int hardness = partition.GetHardness(tilePositionInPartition);
                float breakRatio = 1f - ((float)hardness) / tileItem.tileOptions.hardness;
                closedChunkSystem.BreakIndicator.SetBreak(breakRatio,cellPosition,tileItem,partition.GetBaseData(tilePositionInPartition));
            } else {
                closedChunkSystem.BreakIndicator.RemoveBreak(cellPosition);
            }
            return broken;
        }

        public BaseTileData GetBaseTileData(int x, int y)
        {
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return null;
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            return partition.GetBaseData(positionInPartition);
        }
        

        protected override void SetTile(int x, int y,TileItem tileItem) {
            PlaceTileInTilemap(tilemap,tileItem,new Vector3Int(x,y,0),null);   
        }

        protected void PlaceTileInTilemap(Tilemap placementMap, TileItem tileItem, Vector3Int placementPositon, IChunkPartition partition)
        {
            TileBase tileBase = tileItem.tile;
            if (ReferenceEquals(tileBase,null)) return;
            
            Vector2Int position = new Vector2Int(placementPositon.x, placementPositon.y);
            if (partition == null)
            {
                partition = GetPartitionAtPosition(position);
            }
            if (partition == null) return;
            
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            bool rotatable = tileItem.tileOptions.rotatable;
            TransmutableItemMaterial transmutableItemMaterial = tileItem.tileOptions.TransmutableColorOverride;
            
            SetTileItemTile(placementMap, tileBase, placementPositon, rotatable, baseTileData);
            
            // Don't use get tile color for performance
            if (transmutableItemMaterial)
            {
                placementMap.SetTileFlags(placementPositon, TileFlags.None);
                placementMap.SetColor(placementPositon,tileItem.tileOptions.TransmutableColorOverride.color);
            } else if (tileItem.tileOptions.TileColor)
            {
                placementMap.SetTileFlags(placementPositon, TileFlags.None);
                placementMap.SetColor(placementPositon,tileItem.tileOptions.TileColor.GetColor());
            } 
        }
        protected Color GetTileColor(TileItem tileItem)
        {
            if (tileItem.tileOptions.TransmutableColorOverride)
                return tileItem.tileOptions.TransmutableColorOverride.color;
            if (tileItem.tileOptions.TileColor)
                return tileItem.tileOptions.TileColor.GetColor();
            return Color.white;
        }
        
        protected void SetTileItemTile(Tilemap placementTilemap, TileBase tileBase, Vector3Int position, bool rotatable, BaseTileData baseTileData)
        {
            if (tileBase is IStateTileSingle stateTile) {
                tileBase = stateTile.GetTileAtState(baseTileData.state);
            } 
            if (!rotatable) 
            {
                placementTilemap.SetTile(position,tileBase);
                return;
            }
            if (tileBase is IStateRotationTile stateRotationTile) {
                placementTilemap.SetTile(position, stateRotationTile.getTile(baseTileData.rotation,baseTileData.mirror));
                return;
            }
   
            TilePlaceUtils.RotateTileInMap(placementTilemap, tileBase, position, baseTileData.rotation,baseTileData.mirror);
        }

        protected void PlaceOverlayTile(TileOverlay tileOverlay, Tilemap overlayTileMap, Vector3Int vector3Int, TileItem tileItem, BaseTileData baseTileData )
        {
            var overlayTile = tileOverlay.GetTile();
            SetTileItemTile(overlayTileMap,overlayTile, vector3Int, tileItem.tileOptions.rotatable, baseTileData);
            overlayTileMap.SetTileFlags(vector3Int, TileFlags.None); // Required to get color to work
            overlayTileMap.SetColor(vector3Int,tileOverlay.GetColor());
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
            partition?.SetTile(positionInPartition,GetTileMapType().ToLayer(),item);
        }

        public TileItem getTileItem(Vector2Int cellPosition) {
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            if (partition == null) {
                return null;
            }
            Vector2Int positionInPartition = GetTilePositionInPartition(cellPosition);
            TileItem tileItem = partition.GetTileItem(positionInPartition,GetTileMapType().ToLayer());
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
            TileItem tileItem = partition.GetTileItem(positionInPartition,GetTileMapType().ToLayer());
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

        public virtual void IterateRotatableTile(Vector2Int position, int direction, BaseTileData baseTileData)
        {
            TileItem tileItem = getTileItem(position);
            int newRotation = Buildinator.CalculateNewRotation(baseTileData.rotation, direction);
           
            Vector2 worldPosition = tilemap.CellToWorld((Vector3Int)position);
            
            FloatIntervalVector exclusion = TileHelper.getRealCoveredArea(worldPosition, Global.GetSpriteSize(tileItem.GetSprite()), baseTileData.rotation);
            if (!TilePlaceUtils.BaseTilePlaceable(tileItem, worldPosition, closedChunkSystem, newRotation, exclusion))
            {
                return;
            }

            Vector2Int spriteSize = Global.GetSpriteSize(tileItem.GetSprite());
            bool updateOnRotate = spriteSize.x != spriteSize.y;
            if (updateOnRotate) UpdateListeners(position,tileItem);
            
            TilePlaceUtils.ClearTilesOnPlace(tileItem,worldPosition,newRotation);
            var (partition, positionInPartition) = ((ILoadedChunkSystem)closedChunkSystem).GetPartitionAndPositionAtCellPosition(position);

            ITileEntityInstance tileEntityInstance =  partition.GetTileEntity(positionInPartition);
            IConduitPortTileEntity portTileEntity = tileEntityInstance as IConduitPortTileEntity;
            ConduitClosedChunkSystem conduitClosedChunkSystem = closedChunkSystem as ConduitClosedChunkSystem;
            bool updatePort = portTileEntity != null && !ReferenceEquals(conduitClosedChunkSystem, null);
            
            // Important to delete the tile entity before the rotation changes
            if (updatePort) conduitClosedChunkSystem.TileEntityDeleteUpdate(position);
            
            baseTileData.rotation = newRotation;
            SetTile(position.x,position.y,tileItem);
            if (updatePort) conduitClosedChunkSystem.TileEntityPlaceUpdate(tileEntityInstance);  
            if (updateOnRotate) UpdateListeners(position,tileItem);
            
            closedChunkSystem.BreakIndicator.RotateTile(position,direction);
        }

        public virtual void IterateHammerTile(Vector2Int position, int direction)
        {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return;
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            
            TileItem tileItem = getTileItem(position);
            if (ReferenceEquals(tileItem, null)) return;
            if (tileItem.tile is not HammerTile hammerTile) return;
            
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            int stateCount = hammerTile.GetStateAmount();
            int newState = ((baseTileData.state+direction) % stateCount + stateCount) % stateCount;
            baseTileData.state = newState;
            
            SetTile(position.x,position.y,tileItem);
            TileBase tile = tileItem.tile;
            
            if (tile is IStateTileSingle stateTile)
            {
                tile = stateTile.GetTileAtState(baseTileData.state);
            }

            if (tile is IStateRotationTile)
            {
                // Switching to a non-state tile will fuck up the rotation of state tiles so have to reset it to 0
                TilePlaceUtils.SetTileMapMatrix(tilemap, new Vector3Int(position.x,position.y,0), 0,false);
            }
            
            closedChunkSystem.BreakIndicator.RemoveBreak(position);
            int hardness = partition.GetHardness(tilePositionInPartition);
            float breakRatio = 1f - ((float)hardness) / tileItem.tileOptions.hardness;
            closedChunkSystem.BreakIndicator.SetBreak(breakRatio,position,tileItem,baseTileData);
        }
        
        /// <summary>
        /// TileUpdate check if placement position restrictions are still satisfied.
        /// Note: Currently there is a "bug" where this doesn't work for large tiles (EG 32x16). Not sure if its worth
        /// implementing or not. Because of the way tiles are placed it still works for doors and that's all that really matters.
        /// </summary>
        /// <param name="position"></param>
        public void TileUpdate(Vector2Int position)
        {
            List<(Vector2Int,Direction)> directions = new List<(Vector2Int,Direction)>
            {
                (Vector2Int.left,Direction.Right),
                (Vector2Int.right,Direction.Left),
                (Vector2Int.down,Direction.Up),
                (Vector2Int.up,Direction.Down),
            };
            
            foreach (var (vectorDirection, adjDirection) in directions)
            {
                Vector2Int adjacentPosition = vectorDirection + position;
                TileItem tileItem = getTileItem(position+vectorDirection);
                if (!tileItem) continue;
                
                TilePlacementOptions placementOptions = tileItem.tileOptions?.placementRequirements;
                if (placementOptions == null || !placementOptions.BreakWhenBroken) continue;
                
                IChunkPartition partition = GetPartitionAtPosition(adjacentPosition);
                if (partition == null) continue;
                Vector2Int positionInPartition = GetTilePositionInPartition(adjacentPosition);
                BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
                int state = baseTileData.state;
                if (tileItem.tile is IDirectionStateTile directionStateTile)
                {
                    Direction? direction = directionStateTile.GetDirection(state);
                    if (direction == null || direction.Value != adjDirection) continue;
                    if (!UpdateDirectionalStateTile(direction.Value,placementOptions)) continue;
                    BreakAndDropTile(adjacentPosition,true);
                    continue;
                }
                
                if (placementOptions.Above && adjDirection == Direction.Down)
                {
                    BreakAndDropTile(adjacentPosition,true);
                } else if (placementOptions.Below && adjDirection == Direction.Up)
                {
                    BreakAndDropTile(adjacentPosition,true);
                }
            }
        }

        public void FluidUpdate(Vector2Int cellPosition)
        {
            TileItem tileItem = getTileItem(cellPosition);
            if (!tileItem || !tileItem.tileOptions.fluidBreakable) return;
            BreakAndDropTile(cellPosition, true);
        }

        private bool UpdateDirectionalStateTile(Direction direction, TilePlacementOptions tilePlacementOptions)
        {
            switch (direction)
            {
                case Direction.Left:
                case Direction.Right:
                    return tilePlacementOptions.Side;
                case Direction.Down:
                    return tilePlacementOptions.Below;
                case Direction.Up:
                    return tilePlacementOptions.Above;
                default:
                    return false;
            }
        }
        
        public virtual OutlineTileMapCellData GetOutlineCellData(Vector3Int cellPosition)
        {
            TileItem tileItem = (TileItem)GetItemObject(new  Vector2Int(cellPosition.x, cellPosition.y));
            Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterialNullSafe(tileItem?.tileOptions.TransmutableColorOverride);
            TileOverlay tileOverlay = tileItem?.tileOptions.overlay;
            TileBase overlayTile = GetOverlayInformation(tileOverlay, cellPosition);
            return new OutlineTileMapCellData(
                tilemap.GetTile(cellPosition), 
                null,
                tilemap.GetTransformMatrix(cellPosition).rotation,
                tilemap.GetTransformMatrix(cellPosition).rotation,
                tilemap.GetColor(cellPosition),
                material,
                overlayTile,
                tileOverlay
            );
        }
        
        private TileBase GetOverlayInformation(TileOverlay overlay, Vector3Int position)
        {
            if (!overlay) return null;

            if (overlay is not IShaderTileOverlay shaderTileOverlay) return primaryShaderTilemap?.GetTilemap(null)?.GetTile(position);
            
            Tilemap shaderMap = primaryShaderTilemap.GetTilemap(shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World));
            return shaderMap.GetTile(position);
        }
    }
}
















