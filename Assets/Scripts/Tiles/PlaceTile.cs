using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Chunks;
using System;
using System.Linq;
using TileMaps.Layer;
using TileMaps.Type;
using TileMaps.Conduit;
using Chunks.Systems;
using Chunks.Partitions;
using Conduits;
using Conduits.Ports;
using Conduits.Systems;
using Dimensions;
using Tiles;
using Fluids;
using Item.Slot;
using Items;
using Items.Tags;
using Player;
using TileEntity.Instances.CompactMachines;
using TileEntity.MultiBlock;
using TileMaps.Previewer;
using UnityEngine.Tilemaps;

namespace TileMaps.Place {
    public static class PlaceTile
    {
        private const float BLOCK_SIZE = 0.5f;
        private enum GridLocation {
            UpLeft,
            UpRight,
            DownRight,
            DownLeft,
            Center
        }
        
        public static void RotateTileInMap(Tilemap tilemap, TileBase tileBase, Vector3Int cellPosition, int rotation, bool mirror)
        {
            tilemap.SetTile(cellPosition,null); // This is required to reset the transform matrix in the tilemap to the base 
            tilemap.SetTile(cellPosition,tileBase); 
            SetTileMapMatrix(tilemap, cellPosition, rotation, mirror);
        }

        public static void SetTileMapMatrix(Tilemap tilemap, Vector3Int cellPosition, int rotation, bool mirror)
        {
            Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(cellPosition);
            
            int rotationDeg = 90 * rotation;
            transformMatrix.SetTRS(
                GetOffsetPosition(ref transformMatrix,rotation),
                mirror
                    ? Quaternion.Euler(0f, 180f, rotationDeg)
                    : Quaternion.Euler(0f, 0f, rotationDeg),
                Vector3.one
            );
            tilemap.SetTransformMatrix(cellPosition, transformMatrix);
        }
        
        private static Vector3 GetOffsetPosition(ref Matrix4x4 matrix, int rotation)
        {
            if (rotation % 2 == 0)
            {
                return matrix.GetPosition();
            }
            
            Vector3 tileOffset = matrix.GetPosition();
            (tileOffset.x, tileOffset.y) = (tileOffset.y, tileOffset.x);
            return tileOffset;
        }
        /**
        Conditions:
        i) no tileBlock within sprite size.
        ii) no tileObject within sprite size.
        iii) tileblock below, above, left, or right, or a tilebackground at the location.
        **/
        public static bool PlaceFromWorldPosition(PlayerScript playerScript, ItemSlot itemSlot, Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem, bool checkConditions = true)
        {
            switch (itemSlot?.itemObject)
            {
                case TileItem tileItem:
                {
                    TileMapType tileMapType = tileItem.tileType.toTileMapType();
                    TilePlacementData tilePlacementData = new TilePlacementData(playerScript.TilePlacementOptions.Rotation, playerScript.TilePlacementOptions.State);
                    if (!TilePlacable(tilePlacementData, tileItem, worldPlaceLocation, closedChunkSystem)) return false;
                    
                    placeTile(tileItem,worldPlaceLocation,closedChunkSystem.GetTileMap(tileMapType),closedChunkSystem, placementData: tilePlacementData, itemTagCollection: itemSlot?.tags);
                    return true;
                }
                case ConduitItem conduitItem when closedChunkSystem is not ConduitTileClosedChunkSystem:
                    return false;
                case ConduitItem conduitItem:
                {
                    TileMapType tileMapType = conduitItem.GetConduitType().ToTileMapType();
                    IWorldTileMap conduitMap = closedChunkSystem.GetTileMap(tileMapType);
                    if (conduitMap is not ConduitTileMap conduitTileMap) {
                        return false;
                    }
                    if (checkConditions && !ConduitPlacable(conduitItem,worldPlaceLocation,conduitTileMap)) {
                        return false;
                    }
                    PlaceConduit(playerScript, conduitItem,worldPlaceLocation,conduitMap,(ConduitTileClosedChunkSystem)closedChunkSystem);
                    return true;
                }
                case FluidTileItem fluidTileItem:
                {
                    IWorldTileMap fluidMap = closedChunkSystem.GetTileMap(TileMapType.Fluid);
                    if (checkConditions && !fluidPlacable(fluidTileItem,worldPlaceLocation,fluidMap)) {
                        return false;
                    }
                    return placeFluid(fluidTileItem,worldPlaceLocation,fluidMap);
                }
                default:
                    return false;
            }
        }
        
        public static bool TilePlacable(TilePlacementData tilePlacementData, TileItem tileItem,Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem)
        {
            if (!tileItem.tileOptions.placeableInCave && closedChunkSystem.Dim == (int)Dimension.Cave) return false;
            
            TileMapType tileMapType = tileItem.tileType.toTileMapType();
            TileMapLayer layer = tileMapType.ToLayer();
            switch (layer) {
                case TileMapLayer.Base:
                    return BaseTilePlacable(tileItem,worldPlaceLocation, closedChunkSystem, tilePlacementData.Rotation.ToValue());
                case TileMapLayer.Background:
                    return backgroundTilePlacable(tileItem,worldPlaceLocation, closedChunkSystem);
                default:
                    return false;
            }
        }
        public static Vector3Int getItemPlacePosition(ItemObject itemObject, Vector2 position) {
            if (itemObject is TileItem) {
                TileItem tileItem = (TileItem) itemObject;
                return (Vector3Int)PlaceTile.getPlacePosition(tileItem,position.x,position.y);
            } else if (itemObject is ConduitItem) {
                return (Vector3Int) Global.GetCellPositionFromWorld(position);
            }
            return Vector3Int.zero;
        }
        public static bool BaseTilePlacable(TileItem tileItem,Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem, int rotation, FloatIntervalVector exclusion = null)
        {
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPlaceLocation,Global.GetSpriteSize(tileItem.getSprite()),rotation);
            if (exclusion == null)
            {
                if (TileWithinIntervalAreaRange(intervalVector,TileMapLayer.Base, tileItem.tileOptions.placeBreakable)) return false;
            }
            else
            {
                if (TileWithinIntervalAreaRangeExclusion(intervalVector, exclusion, TileMapLayer.Base,tileItem.tileOptions.placeBreakable)) return false;
            }
            
            if (DevMode.Instance.noPlaceLimit) return true;
            
    
            HashSet<Direction> adjacentBlocks = GetActivePerimeter(intervalVector, tileItem.tileType.toTileMapType().ToLayer());
            
            TilePlacementOptions placementOptions = tileItem.tileOptions.placementRequirements;
            
            int pass = 0;
            int restrictions = 0;
            bool atleastOne = placementOptions.AtleastOne;

            List<(bool, Direction[])> values = new List<(bool, Direction[])>
            {
                (placementOptions.Below, new Direction[] {Direction.Down}),
                (placementOptions.Side, new Direction[] {Direction.Left, Direction.Right}),
                (placementOptions.Above, new Direction[] {Direction.Up})
            };
            foreach (var (required, directions) in values)
            {
                if (required) restrictions++; 
                
                foreach (Direction direction in directions)
                {
                    if (!adjacentBlocks.Contains(direction)) continue;
                    pass++;
                    break;
                }
                if (atleastOne && pass > 0) return true;
            }

            if (placementOptions.BackGround)
            {
                restrictions++;
                bool background = backgroundTileWithinRange(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound,
                    closedChunkSystem.GetTileMap(TileMapType.Background)
                );
                if (background) pass++;
            }
            
            if (restrictions == 0) return pass > 0;
            if (atleastOne && pass > 0) return true;
            return pass > 0 && pass >= restrictions;
        }   

    
        /**
        Conditions:
        i) no tileBackground within sprite size.
        ii) tileBackground below, above, left, or right, or a tileblock at the location.
        **/
        private static bool backgroundTilePlacable(TileItem tileItem,Vector2 worldPosition, ClosedChunkSystem closedChunkSystem) { 
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPosition,Global.GetSpriteSize(tileItem.getSprite()),0);
            if (backgroundTileWithinRange(
                intervalVector.X.LowerBound,
                intervalVector.X.UpperBound,
                intervalVector.Y.LowerBound,
                intervalVector.Y.UpperBound,
                closedChunkSystem.GetTileMap(TileMapType.Background)
            )) {
                return false;
            }
            if (DevMode.Instance.noPlaceLimit) {
                return true;
            }
            
            return 
                tileWithinParameter(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound,
                    TileMapLayer.Base.ToRaycastLayers()) 
                || backgroundWithinParameter(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound,
                    closedChunkSystem.GetTileMap(TileMapType.Background))
                || tileWithinRange(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound,
                    TileMapLayer.Base.ToRaycastLayers());
        }

        /// <summary>
        /// Places a Tile at a given location.
        /// </summary>
        /// <param name="tileItem"></param>
        /// <param name="worldPosition"></param>
        /// <param name="iWorldTileMap"></param>
        /// <param name="presetTileEntity"></param>
        /// <param name="useOffset"></param>
        /// <param name="closedChunkSystem"></param>
        /// <param name = "id"> The id of the TileBlock to be placed </param>
        /// <param name = "x"> The x position to be placed at</param>
        /// <param name = "y"> The y position to be placed at </param>
        /// <param name = "containerName"> The name of the GameObjectContainer which the tile is to be placed in </param>
        public static void placeTile(TileItem tileItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap, ClosedChunkSystem closedChunkSystem, ITileEntityInstance presetTileEntity = null, TilePlacementData placementData = null, ItemTagCollection itemTagCollection = null) {
            if (iWorldTileMap == null || ReferenceEquals(tileItem,null)) {
                return;
            }
            if (!closedChunkSystem.LocalWorldPositionInSystem(worldPosition)) {
                return;
            }

            
            int state = placementData?.State ?? 0;
            if (tileItem.tile is IMousePositionStateTile restrictedTile) {
                state = restrictedTile.GetStateAtPosition(worldPosition);
                bool placeable = state != -1;
                if (!placeable) {
                    return;
                }
            }
            
            Vector2Int placePosition = getPlacePosition(tileItem, worldPosition.x, worldPosition.y);

            var (partition, positionInPartition) = ((ILoadedChunkSystem)closedChunkSystem).GetPartitionAndPositionAtCellPosition(placePosition);
            PlayerTileRotation? tileRotation = placementData?.Rotation;
            int rotation = 0;
            if (tileRotation.HasValue)
            {
                if (tileRotation.Value == PlayerTileRotation.Auto)
                {
                    if (tileItem.tile is HammerTile)
                    {
                        int hammerTileRotation = MousePositionUtils.CalculateHammerTileRotation(worldPosition,placementData?.State??0);
                        if (hammerTileRotation > 0) rotation = hammerTileRotation;
                    }
                }
                else
                {
                    rotation = (int)tileRotation.Value;
                }
            }
            
            BaseTileData baseTileData = new BaseTileData(rotation, state, false);
            partition.SetBaseTileData(positionInPartition, baseTileData);
            partition.SetHardness(positionInPartition,tileItem.tileOptions.hardness);

            ClearTilesOnPlace(tileItem, worldPosition, baseTileData.rotation);
            if (!ReferenceEquals(tileItem.tileEntity, null))
            {
                string initialData = GetPlacementData(tileItem.tileEntity, itemTagCollection);
                PlaceTileEntity(tileItem,closedChunkSystem,iWorldTileMap,worldPosition,presetTileEntity,initialData:initialData);
            }
            iWorldTileMap.PlaceNewTileAtLocation(placePosition.x,placePosition.y,tileItem);
            
            
            if (iWorldTileMap is not WorldTileMap tileGridMap) {
                return;
            }
            TileHelper.tilePlaceTileEntityUpdate(placePosition, tileItem,tileGridMap);
            
        }

        public static void ClearTilesOnPlace(TileItem tileItem, Vector2 worldPosition, int rotation)
        {
            int layers = TileMapLayer.Base.ToRaycastLayers();
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPosition,Global.GetSpriteSize(tileItem.getSprite()),rotation);
            for (float x = intervalVector.X.LowerBound; x <= intervalVector.X.UpperBound; x += 0.5f)
            {
                for (float y = intervalVector.Y.LowerBound; y <= intervalVector.Y.UpperBound; y += 0.5f)
                {
                    Vector2 centered = TileHelper.getRealTileCenter(new Vector2(x, y));
                    var collider = Physics2D.BoxCast(centered, new Vector2(0.48f, 0.48f), 0f, Vector2.zero, Mathf.Infinity, layers).collider;
                    if (ReferenceEquals(collider,null)) continue;
                    Vector2Int cellPosition = Global.GetCellPositionFromWorld(centered);
                    WorldTileMap tileMap = collider.GetComponent<WorldTileMap>();
                    TileItem tile = tileMap?.getTileItem(cellPosition);
                    if (!tile || !tile.tileOptions.placeBreakable) continue;
                    tileMap.BreakAndDropTile(cellPosition, true);
                }
            }
        }

        private static string GetPlacementData(TileEntityObject tileEntity, ItemTagCollection itemTagCollection)
        {
            if (itemTagCollection?.Dict == null) return null;
            if (tileEntity is not ITagPlacementTileEntity tagPlacementTileEntity) return null;
            ItemTag itemTag = tagPlacementTileEntity.GetItemTag();
            if (!itemTagCollection.Dict.TryGetValue(itemTag, out object tagData)) return null;
            
            return tagData as string; // TOOD Chagne this to use some kind of serialization, for now its fine cause compact machines only use strings for data
        }

        public static void PlaceTileEntity(TileItem tileItem, ClosedChunkSystem closedChunkSystem,IWorldTileMap iWorldTileMap, Vector2 offsetPosition, ITileEntityInstance presetTileEntity = null, string initialData = null) {
            Vector2Int chunkPosition = Global.GetChunkFromWorld(offsetPosition);
            Vector2Int tileMapPosition = Global.GetCellPositionFromWorld(offsetPosition);
            Vector2Int partitionPosition = Global.GetPartitionFromWorld(offsetPosition)-chunkPosition*Global.PARTITIONS_PER_CHUNK;
            Vector2Int positionInChunk = tileMapPosition-chunkPosition*Global.CHUNK_SIZE;
            Vector2Int positionInPartition = positionInChunk-partitionPosition*Global.CHUNK_PARTITION_SIZE;
            ILoadedChunk chunk = closedChunkSystem.GetChunk(chunkPosition);
            if (chunk == null) {
                Debug.LogError("Attempted to add TileEntity to null chunk. Chunk [" + chunkPosition.x + "," + chunkPosition.y + "]");
                return;
            }
            IChunkPartition partition = chunk.GetPartition(partitionPosition);
            if (partition == null) {
                Debug.LogError("Attempted to add TileEntity to null partition. Chunk [" + chunkPosition.x + "," + chunkPosition.y + "], Partition:" + partitionPosition.x + "," + partitionPosition.y + "]");
                return;
            }
            
            TileEntityObject tileEntity = tileItem.tileEntity;
            if (ReferenceEquals(tileEntity, null)) return;
            
            ITileEntityInstance tileEntityInstance = presetTileEntity ?? TileEntityUtils.placeTileEntity(tileItem,positionInChunk,chunk,true, assembleMultiblocks: true, data: initialData);
            TileMapLayer layer = iWorldTileMap.GetTileMapType().ToLayer();
            partition.AddTileEntity(layer,tileEntityInstance,positionInPartition);
            
            if (tileEntityInstance is IMultiBlockTileAggregate multiBlockTileAggregate) // This has to be called after the tile entity is added to partition
            {
                TileEntityUtils.UpdateMultiBlockOnPlace(tileEntityInstance, multiBlockTileAggregate);
            }
            if (closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                conduitTileClosedChunkSystem.TileEntityPlaceUpdate(tileEntityInstance);
            }
        }

        

        private static bool PlaceConduit(PlayerScript playerScript, ConduitItem conduitItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap, ConduitTileClosedChunkSystem closedChunkSystem)
        {
            Vector2Int placePosition = iWorldTileMap.WorldToTileMapPosition(worldPosition);
            ConduitType conduitType = conduitItem.GetConduitType();
            IConduitSystemManager conduitSystemManager = closedChunkSystem.GetManager(conduitType);
            EntityPortType entityPortType = conduitSystemManager.GetPortTypeAtPosition(placePosition.x,placePosition.y);
            ITileEntityInstance tileEntity = conduitSystemManager.GetTileEntityAtPosition(placePosition.x,placePosition.y);
            ConduitPlacementOptions placementOptions = playerScript.ConduitPlacementOptions;
            placementOptions.UpdatePlacementType(conduitItem.GetConduitType());
            int state = conduitSystemManager.GetNewState(placePosition,placementOptions,conduitItem.id);
            placementOptions.AddPlacementPosition(placePosition);
            IConduit conduit = ConduitFactory.CreateNew(conduitItem,entityPortType,placePosition.x,placePosition.y,state,tileEntity);
            conduitSystemManager.SetConduit(placePosition.x,placePosition.y,conduit);
            iWorldTileMap.PlaceNewTileAtLocation(placePosition.x,placePosition.y,conduitItem);
            
            return true;
        }

        private static bool placeFluid(FluidTileItem fluidTileItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap) {
            if (iWorldTileMap is not FluidTileMap fluidTileMap) {
                return false;
            }
            Vector2Int placePosition = Global.GetCellPositionFromWorld(worldPosition);
            iWorldTileMap.PlaceNewTileAtLocation(placePosition.x,placePosition.y,fluidTileItem);
            return true;
        }

        

        public static bool ConduitPlacable(ConduitItem conduitItem, Vector2 worldPosition, ConduitTileMap conduitTileMap) {
            Vector2Int tileMapPosition = conduitTileMap.WorldToTileMapPosition(worldPosition);
            return !conduitTileMap.HasTile(tileMapPosition);
        }

        private static bool fluidPlacable(FluidTileItem fluidTileItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap) {
            Vector2Int tileMapPosition = iWorldTileMap.WorldToTileMapPosition(worldPosition);
            if (raycastTileInBox(worldPosition, TileMapLayer.Base.ToRaycastLayers(), true)) return false;
            return !iWorldTileMap.HasTile(tileMapPosition);
        }

        public static UnityEngine.Vector2Int getPlacePosition(ItemObject item, float x, float y) {
            return new UnityEngine.Vector2Int(snap(x), snap(y));
        }

        public static bool tileInDirection(Vector2 position, Direction direction, TileMapLayer layer, bool requireFlat = true)
        {
            float centeredX = (float)Math.Floor(position.x / Global.TILE_SIZE) * Global.TILE_SIZE +
                              Global.TILE_SIZE / 2f;
            float centeredY = (float)Math.Floor(position.y / Global.TILE_SIZE) * Global.TILE_SIZE +
                              Global.TILE_SIZE / 2f;
            Vector2 centered = new Vector2(centeredX,centeredY);
            if (direction == Direction.Center)
            {
                return raycastTileInBox(centered,layer.ToRaycastLayers());  
            }
            if (requireFlat)
            {
                return RaycastTileInLine(direction,centered,layer.ToRaycastLayers());
            }

            return direction switch
            {
                Direction.Down => raycastTileInBox(centered + Vector2.down * Global.TILE_SIZE, layer.ToRaycastLayers()),
                Direction.Up => raycastTileInBox(centered + Vector2.up * Global.TILE_SIZE, layer.ToRaycastLayers()),
                Direction.Left => raycastTileInBox(centered + Vector2.left * Global.TILE_SIZE, layer.ToRaycastLayers()),
                Direction.Right => raycastTileInBox(centered + Vector2.right * Global.TILE_SIZE,
                    layer.ToRaycastLayers()),
                _ => false
            };
        }
        
        public static int snap(float value) {
            return Mathf.FloorToInt(2*value);
        }

        public static float mod(float x, float m) {
        return (x%m + m)%m;
        }

        public static float modInt(float x, int m) {
        return (x%m + m)%m;
        }
        /// <summary>
        /// raycasts a given position in a 0.5f, 0.5f box.
        /// </summary>
        public static bool raycastTileInBox(Vector2 position, int layers, bool ignorePlaceBreakable = false)
        {
            var collider = Physics2D.BoxCast(position, new Vector2(0.48f, 0.48f), 0f, Vector2.zero, Mathf.Infinity, layers).collider;
            if (ReferenceEquals(collider,null)) return false;
            if (ignorePlaceBreakable) return true;
            WorldTileMap tileMap = collider.GetComponent<WorldTileMap>();
            TileItem tile = tileMap?.GetTileItem(position);
            if (!tile) return true; // Return true here since only 16x16 tiles should ever be placebreakable. 
            return !tile.tileOptions.placeBreakable;
        }

        public static bool RaycastTileInLine(Direction direction, Vector2 position, int layers) {
            float width = Global.TILE_SIZE-0.02f;
            float directionDif = width/2f;
            float directionSize = width/8f; // Direciton size is about 2 pixels
            Vector2Int iterator = Vector2Int.one;
            const int SECTIONS = 4;
            float sectionSize = width / SECTIONS;
            Vector2 castSize = sectionSize * Vector2.one;
            Vector2 positionOffset = Vector2.zero;
            switch (direction)
            {
                case Direction.Left:
                case Direction.Right:
                    iterator.y = SECTIONS;
                    castSize.x = directionSize;
                    positionOffset += width / 2f * Vector2.up;
                    break;
                case Direction.Down:
                case Direction.Up:
                    iterator.x = SECTIONS;
                    castSize.y = directionSize;
                    positionOffset += width / 2f * Vector2.right;
                    break;
            }
            
            Vector2 difVector = Vector2.zero;
            switch (direction)
            {
                case Direction.Left:
                    difVector = Vector2.left;
                    break;
                case Direction.Right:
                    difVector = Vector2.right;
                    break;
                case Direction.Down:
                    difVector = Vector2.down;
                    break;
                case Direction.Up:
                    difVector = Vector2.up;
                    break;
            }
            
            for (int xi = 0; xi < iterator.x; xi++)
            {
                for (int yi = 0; yi < iterator.y; yi++)
                {
                    Vector2 adjPosition = position + sectionSize * new Vector2(xi, yi);
                    Vector2 castPosition = adjPosition + difVector * directionDif-positionOffset;
                    Debug.DrawRay(castPosition, castSize, Color.red, 1f);
                    if (!Physics2D.BoxCast(castPosition, castSize, 0f, Vector2.zero, Mathf.Infinity, layers).collider) return false;
                }
            }

            return true;
        }

        private static bool PerimeterFiled(float x, float y, int layers, Direction direction)
        {
            Vector2 offset = Vector2.zero;
            float offsetSize = Global.TILE_SIZE / 4f;
            Vector2Int iterator = Vector2Int.one;
            const int SECTIONS = 2;
            switch (direction)
            {
                case Direction.Left:
                    offset.x = -offsetSize;
                    iterator.x = SECTIONS;
                    x -= Global.TILE_SIZE;
                    break;
                case Direction.Right:
                    offset.x = offsetSize;
                    iterator.x = SECTIONS;
                    x += Global.TILE_SIZE;
                    break;
                case Direction.Down:
                    offset.y = -offsetSize;
                    iterator.y = SECTIONS;
                    y -= Global.TILE_SIZE;
                    break;
                case Direction.Up:
                    offset.y = offsetSize;
                    iterator.y = SECTIONS;
                    y += Global.TILE_SIZE;
                    break;
                case Direction.Center:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            float castSize = Global.TILE_SIZE / SECTIONS;
            Vector2 boxSize = (castSize - 0.01f) * Vector2.one;
            Vector2 position = new Vector2(x,y);
            for (int xi = 0; xi < iterator.x; ++xi) {
                for (int yi = 0; yi < iterator.y; ++yi)
                {
                    Vector2 linePosition = position + castSize * new Vector2(xi,yi) + offset;
                    Debug.Log(linePosition);
                    var collider = Physics2D.BoxCast(linePosition,boxSize, 0f, Vector2.zero, Mathf.Infinity, layers).collider;
                    if (ReferenceEquals(collider,null)) return false;
                }
            }
            return true;
        }
        /**
        returns true if there is a tile within the range, inclusive
        **/
        private static bool tileWithinRange(float minX, float maxX, float minY, float maxY, int layers,
            bool ignorePlaceBreakable = false) {
            for (float x = minX; x <= maxX; x += Global.TILE_SIZE) {
                for (float y = minY; y <= maxY; y += Global.TILE_SIZE) {
                    if (raycastTileInBox(new Vector2(x,y), layers,ignorePlaceBreakable)) {
                        return true;
                    }
                }
            }
            return false;
        }
        /// Summary:
        ///     Since tilebackgrounds can "overflow" into other grids, must check each quadrant of the grid.
        ///     Returns true iff every raycast background hits 
        private static bool backgroundTileWithinRange(float minX, float maxX, float minY, float maxY, IWorldTileMap worldTileMap)
        {
            Tilemap tilemap = worldTileMap.GetTilemap();
            for (float x = minX; x <= maxX; x += 1/2f) {
                for (float y = minY; y <= maxY; y += 1/2f)
                {
                    Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x, y, 0));
                    cellPosition.z = 0;
                    TileBase tile = tilemap.GetTile(cellPosition);
                    if (!ReferenceEquals(tile, null)) return true;
                }
            }
            return false;
        }

        
        private static bool tileWithinParameter(float minX, float maxX, float minY, float maxY, int layers) {
            return (
                tileWithinRange(minX-BLOCK_SIZE,minX-BLOCK_SIZE,minY,maxY,layers) ||
                tileWithinRange(maxX+BLOCK_SIZE,maxX+BLOCK_SIZE,minY,maxY,layers) ||
                tileWithinRange(minX,maxX,minY-BLOCK_SIZE,minY-BLOCK_SIZE,layers) ||
                tileWithinRange(minX,maxX,maxY+BLOCK_SIZE,maxY+BLOCK_SIZE,layers));
        }
        
        private static bool backgroundWithinParameter(float minX, float maxX, float minY, float maxY, IWorldTileMap worldTileMap) {
            return (
                backgroundTileWithinRange(minX-BLOCK_SIZE,minX-BLOCK_SIZE,minY,maxY,worldTileMap) ||
                backgroundTileWithinRange(maxX+BLOCK_SIZE,maxX+BLOCK_SIZE,minY,maxY,worldTileMap) ||
                backgroundTileWithinRange(minX,maxX,minY-BLOCK_SIZE,minY-BLOCK_SIZE,worldTileMap) ||
                backgroundTileWithinRange(minX,maxX,maxY+BLOCK_SIZE,maxY+BLOCK_SIZE,worldTileMap));
        }

        private static bool TileWithinIntervalAreaRange(FloatIntervalVector floatIntervalVector, TileMapLayer layer,
            bool placeBreakable) {
            return tileWithinRange(
                floatIntervalVector.X.LowerBound,
                floatIntervalVector.X.UpperBound,
                floatIntervalVector.Y.LowerBound,
                floatIntervalVector.Y.UpperBound,
                layer.ToRaycastLayers(), placeBreakable);
        }
        
        private static bool TileWithinIntervalAreaRangeExclusion(FloatIntervalVector floatIntervalVector, FloatIntervalVector exclusion, TileMapLayer layer, bool ignorePlaceBreakable = false) {
            float minX = floatIntervalVector.X.LowerBound;
            float maxX = floatIntervalVector.X.UpperBound;
            float minY = floatIntervalVector.Y.LowerBound;
            float maxY = floatIntervalVector.Y.UpperBound;
            
            for (float x = minX; x <= maxX; x += 1/2f) {
                for (float y = minY; y <= maxY; y += 1/2f) {
                    if (x >= exclusion.X.LowerBound && x <= exclusion.X.UpperBound && y >= exclusion.Y.LowerBound && y <= exclusion.Y.UpperBound) continue;
                    if (raycastTileInBox(new Vector2(x,y), layer.ToRaycastLayers(),ignorePlaceBreakable)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static HashSet<Direction> GetActivePerimeter(FloatIntervalVector floatIntervalVector, TileMapLayer layer)
        {
            int layers = layer.ToRaycastLayers();
            float minX = floatIntervalVector.X.LowerBound;
            float maxX = floatIntervalVector.X.UpperBound;
            float minY = floatIntervalVector.Y.LowerBound;
            float maxY = floatIntervalVector.Y.UpperBound;
            HashSet<Direction> directions = new HashSet<Direction>();
            if (tileWithinRange(minX - BLOCK_SIZE, minX - BLOCK_SIZE, minY, maxY, layers))
                directions.Add(Direction.Left);
            if (tileWithinRange(maxX + BLOCK_SIZE, maxX + BLOCK_SIZE, minY, maxY, layers))
                directions.Add(Direction.Right);
            if (tileWithinRange(minX, maxX, minY - BLOCK_SIZE, minY - BLOCK_SIZE, layers))
                directions.Add(Direction.Down);
            
            if (tileWithinRange(minX,maxX,maxY+BLOCK_SIZE,maxY+BLOCK_SIZE,layers))
                directions.Add(Direction.Up);
            return directions;

        }
    }
}

public enum Direction {
    Left,
    Right,
    Down,
    Up,
    Center
}

public enum DirectionState
{
    Left = 1,
    Right = 2,
    Down = 4,
    Up = 8,
}