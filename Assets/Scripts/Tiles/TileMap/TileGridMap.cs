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
using Tiles.Options.Overlay;

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
    public class WorldTileGridMap : AbstractIWorldTileMap<TileItem>, ITileGridMap, IChiselableTileMap, IRotatableTileMap, IHammerTileMap, IConditionalHitableTileMap, ITileMapListener
    {
        public const float OVERLAY_Z = -3f;
        private Tilemap overlayTileMap;
        private ShaderOverlayTilemapManager shaderOverlayTilemapManager;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            GameObject overlayTileMapObject = new GameObject("OverlayTileMap");
            overlayTileMapObject.transform.SetParent(transform,false);
            overlayTileMap = overlayTileMapObject.AddComponent<Tilemap>();
            overlayTileMapObject.AddComponent<TilemapRenderer>();
            overlayTileMapObject.transform.localPosition = new Vector3(0, 0, OVERLAY_Z);
            shaderOverlayTilemapManager = new ShaderOverlayTilemapManager(transform);

        }

        protected override void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            SpawnItemEntity(new ItemSlot(itemObject,amount,null), hitTilePosition);
        }
        
        protected void SpawnItemEntity(ItemSlot itemSlot, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);

            float realXPosition = transform.position.x + hitTilePosition.x / 2f;
            float realYPosition = transform.position.y + hitTilePosition.y / 2f;
            Sprite[] itemSprites = itemSlot.itemObject.getSprites();
            if (itemSprites.Length == 0) {
                Debug.LogError("Tried to spawn item with no sprite");
                return;
            }
            Vector2 spriteSize =  Global.GetSpriteSize(itemSprites[0]);
            if (PlaceTile.mod(spriteSize.x,2) == 0) {
                realXPosition += 0.25f;
            }
            if (PlaceTile.mod(spriteSize.y,2) == 0) {
                realYPosition += 0.25f;
            }
            ItemEntityFactory.SpawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.GetEntityContainer());
        }
        
        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            Vector2Int hitPosition = WorldToTileMapPosition(position);
            int maxSearchWidth = 16;
            int searchWidth = 1;
            while (searchWidth < maxSearchWidth) {
                if (Global.ModInt(searchWidth,2) == 0) {
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
                TileMapLayer layer = type.toLayer();
                partition.BreakTileEntity(layer,tilePositionInPartition);
                DeleteTileEntityFromConduit(position);
            }
            Vector3Int vector = new Vector3Int(position.x,position.y,0);
            tilemap.SetTile(vector, null);
            if (overlayTileMap.GetTile(vector)) overlayTileMap.SetTile(vector, null);
            TileItem tileItem = partition.GetTileItem(tilePositionInPartition, TileMapLayer.Base);
            var tileOverlay = tileItem.tileOptions.Overlay;
            if (tileOverlay)
            {
                Tilemap placementTilemap = GetOverlayTileMap(tileOverlay);
                placementTilemap.SetTile(vector,null);
            }
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

        private Tilemap GetOverlayTileMap(TileOverlay tileOverlay)
        {
            if (tileOverlay is not IShaderTileOverlay shaderTileOverlay) return overlayTileMap;
            Material shaderMaterial = shaderTileOverlay.GetMaterial();
            return !shaderMaterial ? overlayTileMap : shaderOverlayTilemapManager.GetTileMap(shaderMaterial);
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
            Vector2Int spriteSize = Global.GetSpriteSize(tileItem.getSprite());
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

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            Vector3Int vector = new Vector3Int(x,y,0);
            if (overlayTileMap.GetTile(vector)) overlayTileMap.SetTile(vector, null);
        }
        

        protected override void SetTile(int x, int y,TileItem tileItem) {
            TileBase tileBase = tileItem.tile;
            if (ReferenceEquals(tileBase,null)) return;
            
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            Vector3Int vector3Int = new Vector3Int(position.x,position.y,0);
            bool rotatable = tileItem.tileOptions.rotatable;
            SetTileItemTile(tilemap, tileBase, vector3Int, rotatable, baseTileData);
            if (tileItem.tileOptions?.TileColor)
            {
                tilemap.SetTileFlags(vector3Int, TileFlags.None);
                tilemap.SetColor(vector3Int,tileItem.tileOptions.TileColor.GetColor());
            }
            else
            {
                tilemap.SetColor(vector3Int,Color.white);
                tilemap.SetTileFlags(vector3Int, TileFlags.LockColor);
            }
            
            var tileOverlay = tileItem.tileOptions?.Overlay;
            if (!tileOverlay) return;
            var overlayTile = tileOverlay.GetTile();
            Tilemap placementTilemap = GetOverlayTileMap(tileOverlay);
            
            SetTileItemTile(placementTilemap, overlayTile, vector3Int, rotatable, baseTileData);
            placementTilemap.SetTileFlags(vector3Int, TileFlags.None); // Required to get color to work
            placementTilemap.SetColor(vector3Int,tileOverlay.GetColor());
        }

        private void SetTileItemTile(Tilemap placementTilemap, TileBase tileBase, Vector3Int position, bool rotatable, BaseTileData baseTileData)
        {
            if (tileBase is IStateTile stateTile) {
                tileBase = stateTile.getTileAtState(baseTileData.state);
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
   
            PlaceTile.RotateTileInMap(placementTilemap, tileBase, position, baseTileData.rotation,baseTileData.mirror);
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
            partition?.SetTile(positionInPartition,GetTileMapType().toLayer(),item);
        }

        public TileItem getTileItem(Vector2Int cellPosition) {
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            if (partition == null) {
                return null;
            }
            Vector2Int positionInPartition = GetTilePositionInPartition(cellPosition);
            TileItem tileItem = partition.GetTileItem(positionInPartition,GetTileMapType().toLayer());
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
            TileItem tileItem = partition.GetTileItem(positionInPartition,GetTileMapType().toLayer());
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
            
            FloatIntervalVector exclusion = TileHelper.getRealCoveredArea(worldPosition, Global.GetSpriteSize(tileItem.getSprite()), baseTileData.rotation);
            if (!PlaceTile.BaseTilePlacable(tileItem, worldPosition, closedChunkSystem, newRotation, exclusion))
            {
                return;
            }

            Vector2Int spriteSize = Global.GetSpriteSize(tileItem.getSprite());
            bool updateOnRotate = spriteSize.x != spriteSize.y;
            if (updateOnRotate) UpdateListeners(position,tileItem);
            
            PlaceTile.ClearTilesOnPlace(tileItem,worldPosition,newRotation);
            var (partition, positionInPartition) = ((ILoadedChunkSystem)closedChunkSystem).GetPartitionAndPositionAtCellPosition(position);

            ITileEntityInstance tileEntityInstance =  partition.GetTileEntity(positionInPartition);
            IConduitPortTileEntity portTileEntity = tileEntityInstance as IConduitPortTileEntity;
            ConduitTileClosedChunkSystem conduitTileClosedChunkSystem = closedChunkSystem as ConduitTileClosedChunkSystem;
            bool updatePort = portTileEntity != null && !ReferenceEquals(conduitTileClosedChunkSystem, null);
            
            // Important to delete the tile entity before the rotation changes
            if (updatePort) conduitTileClosedChunkSystem.TileEntityDeleteUpdate(position);
            
            baseTileData.rotation = newRotation;
            SetTile(position.x,position.y,tileItem);
            if (updatePort) conduitTileClosedChunkSystem.TileEntityPlaceUpdate(tileEntityInstance);  
            if (updateOnRotate) UpdateListeners(position,tileItem);
            
            closedChunkSystem.BreakIndicator.RotateTile(position,direction);
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
            TileBase tile = tileItem.tile;
            
            if (tile is IStateTile stateTile)
            {
                tile = stateTile.getTileAtState(baseTileData.state);
            }

            if (tile is IStateRotationTile)
            {
                // Switching to a non-state tile will fuck up the rotation of state tiles so have to reset it to 0
                PlaceTile.SetTileMapMatrix(tilemap, new Vector3Int(position.x,position.y,0), 0,false);
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
    }

    public class ShaderOverlayTilemapManager
    {
        private const int DEFAULT_COUNT = 3;
        private Transform parentTransform;
        public ShaderOverlayTilemapManager(Transform parent)
        {
            this.parentTransform = parent;
            int i = 0;
            while (i < DEFAULT_COUNT)
            {
                PushNewTilemap();
                i++;
            }
        }   
        
        private void PushNewTilemap()
        {
            int count = unusedTileMaps.Count + materialTileMaps.Count;
            GameObject overlayTileMapObject = new GameObject($"ShaderOverlayTileMap_{count}");
            overlayTileMapObject.transform.SetParent(parentTransform,false);
            var overlayTileMap = overlayTileMapObject.AddComponent<Tilemap>();
            overlayTileMapObject.AddComponent<TilemapRenderer>();
            overlayTileMapObject.transform.localPosition = new Vector3(0, 0, WorldTileGridMap.OVERLAY_Z);
            overlayTileMapObject.gameObject.SetActive(false);
            unusedTileMaps.Push(overlayTileMap);
        }
        private Dictionary<Material, Tilemap> materialTileMaps = new();
        private Stack<Tilemap> unusedTileMaps = new();
        public Tilemap GetTileMap(Material material)
        {
            if (materialTileMaps.TryGetValue(material, out Tilemap tilemap)) return tilemap;
            if (unusedTileMaps.Count == 0)
            {
                PushNewTilemap();
            }
            tilemap = unusedTileMaps.Pop();
            tilemap.gameObject.SetActive(true);
            tilemap.GetComponent<TilemapRenderer>().material = material;
            materialTileMaps[material] = tilemap;

            return tilemap;

        }
    }
}
















