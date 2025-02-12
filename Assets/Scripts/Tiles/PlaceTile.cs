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
using Tiles;
using Fluids;
using Items;
using Player;
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

        public static void tilePlaceUpdate(Vector2Int position, TileMapType tileMapType) {

        }

        public static void RotateTileInMap(Tilemap tilemap, TileBase tileBase, Vector3Int cellPosition, int rotation, bool mirror)
        {
            tilemap.SetTile(cellPosition,null); // This is required to reset the transform matrix in the tilemap to the base 
            tilemap.SetTile(cellPosition,tileBase); 
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
        public static bool PlaceFromWorldPosition(PlayerScript playerScript, ItemObject itemObject, Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem, bool checkConditions = true)
        {
            switch (itemObject)
            {
                case TileItem tileItem:
                {
                    TileMapType tileMapType = tileItem.tileType.toTileMapType();
                    TilePlacementData tilePlacementData = new TilePlacementData(playerScript.TilePlacementOptions.Rotation, playerScript.TilePlacementOptions.State);
                    if (!TilePlacable(tilePlacementData, tileItem, worldPlaceLocation, closedChunkSystem)) return false;
                    
                    placeTile(tileItem,worldPlaceLocation,closedChunkSystem.GetTileMap(tileMapType),closedChunkSystem, placementData: tilePlacementData);
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
        
        public static bool TilePlacable(TilePlacementData tilePlacementData, TileItem tileItem,Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem) {
            TileMapType tileMapType = tileItem.tileType.toTileMapType();
            TileMapLayer layer = tileMapType.toLayer();
            switch (layer) {
                case TileMapLayer.Base:
                    return BaseTilePlacable(tileItem,worldPlaceLocation, closedChunkSystem, tilePlacementData.Rotation);
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
                return (Vector3Int) Global.getCellPositionFromWorld(position);
            }
            return Vector3Int.zero;
        }
        public static bool BaseTilePlacable(TileItem tileItem,Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem, int rotation, FloatIntervalVector exclusion = null)
        {
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPlaceLocation,Global.getSpriteSize(tileItem.getSprite()),rotation);
            if (exclusion == null)
            {
                if (TileWithinIntervalAreaRange(intervalVector,TileMapLayer.Base)) return false;
            }
            else
            {
                if (TileWithinIntervalAreaRangeExclusion(intervalVector, exclusion, TileMapLayer.Base)) return false;
            }
            
            

            if (DevMode.Instance.noPlaceLimit) return true;
            
    
            HashSet<Direction> adjacentBlocks = GetActivePerimeter(intervalVector, tileItem.tileType.toTileMapType().toLayer());
            
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
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPosition,Global.getSpriteSize(tileItem.getSprite()),0);
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
                    TileMapLayer.Base.toRaycastLayers()) 
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
                    TileMapLayer.Base.toRaycastLayers());
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
        public static void placeTile(TileItem tileItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap, ClosedChunkSystem closedChunkSystem, ITileEntityInstance presetTileEntity = null, TilePlacementData placementData = null) {
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

            var (partition, positionInPartition) = ((IChunkSystem)closedChunkSystem).GetPartitionAndPositionAtCellPosition(placePosition);
            int rotation = placementData?.Rotation ?? 0;

            if (tileItem.tile is HammerTile)
            {
                int hammerTileRotation = MousePositionUtils.CalculateHammerTileRotation(worldPosition,placementData?.State??0);
                if (hammerTileRotation > 0) rotation = hammerTileRotation;
            }
            
            BaseTileData baseTileData = new BaseTileData(rotation, state, false);
            partition.SetBaseTileData(positionInPartition, baseTileData);
            partition.SetHardness(positionInPartition,tileItem.tileOptions.hardness);
            
            iWorldTileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,tileItem);
            
            if (!ReferenceEquals(tileItem.tileEntity, null))
            {
                PlaceTileEntity(tileItem,closedChunkSystem,iWorldTileMap,worldPosition,presetTileEntity);
            }
            
            if (iWorldTileMap is not WorldTileGridMap tileGridMap) {
                return;
            }
            TileHelper.tilePlaceTileEntityUpdate(placePosition, tileItem,tileGridMap);
            
        }

        public static void PlaceTileEntity(TileItem tileItem, ClosedChunkSystem closedChunkSystem,IWorldTileMap iWorldTileMap, Vector2 offsetPosition, ITileEntityInstance presetTileEntity = null) {
            Vector2Int chunkPosition = Global.getChunkFromWorld(offsetPosition);
            Vector2Int tileMapPosition = Global.getCellPositionFromWorld(offsetPosition);
            Vector2Int partitionPosition = Global.getPartitionFromWorld(offsetPosition)-chunkPosition*Global.PARTITIONS_PER_CHUNK;
            Vector2Int positionInChunk = tileMapPosition-chunkPosition*Global.CHUNK_SIZE;
            Vector2Int positionInPartition = positionInChunk-partitionPosition*Global.CHUNK_PARTITION_SIZE;
            ILoadedChunk chunk = closedChunkSystem.getChunk(chunkPosition);
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
            
            ITileEntityInstance tileEntityInstance = presetTileEntity ?? TileEntityUtils.placeTileEntity(tileItem,positionInChunk,chunk,true, assembleMultiblocks: true);
            TileMapLayer layer = iWorldTileMap.getType().toLayer();
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
            Vector2Int placePosition = iWorldTileMap.worldToTileMapPosition(worldPosition);
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
            iWorldTileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,conduitItem);
            
            return true;
        }

        private static bool placeFluid(FluidTileItem fluidTileItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap) {
            if (iWorldTileMap is not FluidWorldTileMap fluidTileMap) {
                return false;
            }
            Vector2Int placePosition = Global.getCellPositionFromWorld(worldPosition);
            iWorldTileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,fluidTileItem);
            return true;
        }

        

        public static bool ConduitPlacable(ConduitItem conduitItem, Vector2 worldPosition, ConduitTileMap conduitTileMap) {
            Vector2Int tileMapPosition = conduitTileMap.worldToTileMapPosition(worldPosition);
            return !conduitTileMap.hasTile(tileMapPosition);
        }

        private static bool fluidPlacable(FluidTileItem fluidTileItem, Vector2 worldPosition, IWorldTileMap iWorldTileMap) {
            Vector2Int tileMapPosition = iWorldTileMap.worldToTileMapPosition(worldPosition);
            return !iWorldTileMap.hasTile(tileMapPosition);
        }

        public static UnityEngine.Vector2Int getPlacePosition(ItemObject item, float x, float y) {
            return new UnityEngine.Vector2Int(snap(x), snap(y));
        }

        public static bool tileInDirection(Vector2 position, Direction direction, TileMapLayer layer, bool requireFlat = true) {
            float centeredX = (float)Math.Floor(position.x / 0.5f) * 0.5f + 0.25f;
            float centeredY = (float)Math.Floor(position.y / 0.5f) * 0.5f + 0.25f;
            Vector2 centered = new Vector2(centeredX,centeredY);
            switch (direction) {
                case Direction.Down:
                    if (requireFlat) {
                        return raycastTileInLine(Direction.Down,centered,layer.toRaycastLayers());
                    } else {
                        return raycastTileInBox(centered+Vector2.down*0.5f,layer.toRaycastLayers());
                    }
                    
                case Direction.Up:
                    if (requireFlat) {
                        return raycastTileInLine(Direction.Up,centered,layer.toRaycastLayers());
                    } else {
                        return raycastTileInBox(centered+Vector2.up*0.5f,layer.toRaycastLayers());
                    }
                case Direction.Left:
                    if (requireFlat) {
                        return raycastTileInLine(Direction.Left,centered,layer.toRaycastLayers());
                    } else {
                        return raycastTileInBox(centered+Vector2.left*0.5f,layer.toRaycastLayers());
                    }  
                case Direction.Right:
                    if (requireFlat) {
                        return raycastTileInLine(Direction.Right,centered,layer.toRaycastLayers());
                    } else {
                        return raycastTileInBox(centered+Vector2.right*0.5f,layer.toRaycastLayers());
                    }  
                case Direction.Center:
                    return raycastTileInBox(centered,layer.toRaycastLayers());  
            }
            return false;
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
        public static bool raycastTileInBox(Vector2 position, int layers)
        {
            return !ReferenceEquals(
                Physics2D.BoxCast(position, new Vector2(0.48f, 0.48f), 0f, Vector2.zero, Mathf.Infinity, layers).collider, null);
        }

        public static bool raycastTileInLine(Direction direction, Vector2 position, int layers) {
            float width = 0.48f;
            float directionDif = 0.24f;
            float directionSize = width/8f; // Direciton size is about 2 pixels
            switch (direction) {
                case Direction.Left:
                    return Physics2D.BoxCast(position+Vector2.left*directionDif,new Vector2(directionSize,width),0f,Vector2.zero,Mathf.Infinity,layers).collider != null;
                case Direction.Right:
                    return Physics2D.BoxCast(position+Vector2.right*directionDif,new Vector2(directionSize,width),0f,Vector2.zero,Mathf.Infinity,layers).collider != null;
                case Direction.Up:
                    return Physics2D.BoxCast(position+Vector2.up*directionDif,new Vector2(width,directionSize),0f,Vector2.zero,Mathf.Infinity,layers).collider != null;
                case Direction.Down:
                    return Physics2D.BoxCast(position+Vector2.down*directionDif,new Vector2(width,directionSize),0f,Vector2.zero,Mathf.Infinity,layers).collider != null;
                
            }
            return false;
        }
        
        /**
        returns true if there is a tile within the range, inclusive
        **/
        private static bool tileWithinRange(float minX, float maxX, float minY, float maxY, int layers) {
            for (float x = minX; x <= maxX; x += 1/2f) {
                for (float y = minY; y <= maxY; y += 1/2f) {
                    if (raycastTileInBox(new Vector2(x,y), layers)) {
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

        private static bool TileWithinIntervalAreaRange(FloatIntervalVector floatIntervalVector, TileMapLayer layer) {
            return tileWithinRange(
                floatIntervalVector.X.LowerBound,
                floatIntervalVector.X.UpperBound,
                floatIntervalVector.Y.LowerBound,
                floatIntervalVector.Y.UpperBound,
                layer.toRaycastLayers()
            );
        }
        
        private static bool TileWithinIntervalAreaRangeExclusion(FloatIntervalVector floatIntervalVector, FloatIntervalVector exclusion, TileMapLayer layer) {
            float minX = floatIntervalVector.X.LowerBound;
            float maxX = floatIntervalVector.X.UpperBound;
            float minY = floatIntervalVector.Y.LowerBound;
            float maxY = floatIntervalVector.Y.UpperBound;
            
            for (float x = minX; x <= maxX; x += 1/2f) {
                for (float y = minY; y <= maxY; y += 1/2f) {
                    if (x >= exclusion.X.LowerBound && x <= exclusion.X.UpperBound && y >= exclusion.Y.LowerBound && y <= exclusion.Y.UpperBound) continue;
                    if (raycastTileInBox(new Vector2(x,y), layer.toRaycastLayers())) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static HashSet<Direction> GetActivePerimeter(FloatIntervalVector floatIntervalVector, TileMapLayer layer)
        {
            int layers = layer.toRaycastLayers();
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
