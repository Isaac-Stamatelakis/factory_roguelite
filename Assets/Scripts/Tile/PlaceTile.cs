using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Chunks;
using System;
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

namespace TileMaps.Place {
    public static class PlaceTile {
        private enum GridLocation {
            UpLeft,
            UpRight,
            DownRight,
            DownLeft,
            Center
        }

        public static void tilePlaceUpdate(Vector2Int position, TileMapType tileMapType) {

        }
        /**
        Conditions:
        i) no tileBlock within sprite size.
        ii) no tileObject within sprite size.
        iii) tileblock below, above, left, or right, or a tilebackground at the location.
        **/
        public static bool PlaceFromWorldPosition(ItemObject itemObject, Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem, bool checkConditions = true, TileEntityObject presetTileEntity = null, bool useOffset = true) {
            if (itemObject is TileItem tileItem) {
                if (checkConditions && !tilePlacable(tileItem,worldPlaceLocation)) {
                    return false;
                }
                TileMapType tileMapType = tileItem.tileType.toTileMapType();
                placeTile((TileItem) itemObject,worldPlaceLocation,closedChunkSystem.getTileMap(tileMapType),closedChunkSystem,presetTileEntity,useOffset:useOffset);
                return true;
                
            } else if (itemObject is ConduitItem conduitItem) {
                if (closedChunkSystem is not ConduitTileClosedChunkSystem) {
                    return false;
                }
                TileMapType tileMapType = conduitItem.GetConduitType().ToTileMapType();
                ITileMap conduitMap = closedChunkSystem.getTileMap(tileMapType);
                if (conduitMap == null || conduitMap is not ConduitTileMap) {
                    return false;
                }
                if (checkConditions && !conduitPlacable(conduitItem,worldPlaceLocation,conduitMap)) {
                    return false;
                }
                placeConduit(conduitItem,worldPlaceLocation,conduitMap,(ConduitTileClosedChunkSystem)closedChunkSystem);
                return true;
                
            } else if (itemObject is FluidTileItem fluidTileItem) {
                ITileMap fluidMap = closedChunkSystem.getTileMap(TileMapType.Fluid);
                if (checkConditions && !fluidPlacable(fluidTileItem,worldPlaceLocation,fluidMap)) {
                    return false;
                }
                Vector2 offset = new Vector2(closedChunkSystem.DimPositionOffset.x/2f,closedChunkSystem.DimPositionOffset.y/2f);
                return placeFluid(fluidTileItem,worldPlaceLocation+offset,fluidMap);
            }
            return false;
        }

        private static bool tilePlacable(TileItem tileItem,Vector2 worldPlaceLocation) {
            TileMapType tileMapType = tileItem.tileType.toTileMapType();
            TileMapLayer layer = tileMapType.toLayer();
            switch (layer) {
                case TileMapLayer.Base:
                    return baseTilePlacable(tileItem,worldPlaceLocation);
                case TileMapLayer.Background:
                    return backgroundTilePlacable(tileItem,worldPlaceLocation);
                default:
                    return false;
            }
        }

        public static bool itemPlacable(ItemObject itemObject, Vector2 worldPlaceLocation) {
            if (itemObject is TileItem) {
                TileItem tileItem = (TileItem) itemObject;
                return tilePlacable(tileItem,worldPlaceLocation);
            } else if (itemObject is ConduitItem) {
                ConduitItem conduitItem = ((ConduitItem) itemObject);
                /*
                return conduitPlacable(conduitItem,worldPosition);
                TileMapType tileMapType = conduitItem.getType().toTileMapType();
                TileMapLayer layer = tileMapType.toLayer();
                switch (layer) {
                    case TileMapLayer.Item:
                        break;
                    case TileMapLayer.Fluid:
                        break;
                    case TileMapLayer.Energy:
                        break;
                    case TileMapLayer.Signal:
                        break;
                }
                */
            }
            return false;
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
        private static bool baseTilePlacable(TileItem tileItem,Vector2 worldPlaceLocation) { 
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPlaceLocation,Global.getSpriteSize(tileItem.getSprite()));
            if (tileWithinIntervalAreaRange(intervalVector,TileMapLayer.Base)) {
                return false;
            }

            if (GameObject.Find("Player").GetComponent<DevMode>().noPlaceLimit) {
                return true;
            } 
            if (!tileWithinIntervalParameter(intervalVector,TileMapLayer.Base) && 
                !tileWithinIntervalParameter(intervalVector,TileMapLayer.Background)) {
                return false;
            }
            return true;
        }   

    
        /**
        Conditions:
        i) no tileBackground within sprite size.
        ii) tileBackground below, above, left, or right, or a tileblock at the location.
        **/
        private static bool backgroundTilePlacable(TileItem tileItem,Vector2 worldPosition) { 
            FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(worldPosition,Global.getSpriteSize(tileItem.getSprite()));
            if (backgroundTileWithinRange(
                intervalVector.X.LowerBound,
                intervalVector.X.UpperBound,
                intervalVector.Y.LowerBound,
                intervalVector.Y.UpperBound,
                TileMapLayer.Background.toRaycastLayers()
            )) {
                return false;
            }
            if (GameObject.Find("Player").GetComponent<DevMode>().noPlaceLimit) {
                return true;
            } 
            if (!tileWithinParameter(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound, 
                    TileMapLayer.Base.toRaycastLayers()
                ) && 
                !tileWithinParameter(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound,
                    TileMapLayer.Background.toRaycastLayers()
                ) &&
                !tileWithinRange(
                    intervalVector.X.LowerBound,
                    intervalVector.X.UpperBound,
                    intervalVector.Y.LowerBound,
                    intervalVector.Y.UpperBound, 
                    TileMapLayer.Base.toRaycastLayers()
                )) {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Places a Tile at a given location.
        /// </summary>
        /// <param name = "id"> The id of the TileBlock to be placed </param>
        /// <param name = "x"> The x position to be placed at</param>
        /// <param name = "y"> The y position to be placed at </param>
        /// <param name = "containerName"> The name of the GameObjectContainer which the tile is to be placed in </param>
        private static void placeTile(TileItem tileItem, Vector2 worldPosition, ITileMap tileMap, ClosedChunkSystem closedChunkSystem, TileEntityObject presetTileEntity = null, bool useOffset = true) {
            if (tileMap == null) {
                return;
            }
            if (!closedChunkSystem.localWorldPositionInSystem(worldPosition)) {
                return;
            }
            if (tileItem.tile is IRestrictedTile restrictedTile) {
                int state = restrictedTile.getStateAtPosition(worldPosition,MousePositionFactory.getVerticalMousePosition(worldPosition),MousePositionFactory.getHorizontalMousePosition(worldPosition));
                bool placeable = state != -1;
                if (!placeable) {
                    return;
                }
            }
            Vector2Int offset = closedChunkSystem.DimPositionOffset;
            Vector2 offsetWorld = new Vector2(offset.x/2f,offset.y/2f);
            Vector2 offsetPosition = worldPosition;
            if (useOffset) {
                offsetPosition += offsetWorld;
            }
            UnityEngine.Vector2Int placePosition = getPlacePosition(tileItem, worldPosition.x, worldPosition.y);
            if (useOffset) {
                placePosition += offset;
            }
            tileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,tileItem);
            if (tileItem.tileEntity != null) {
                placeTileEntity(tileItem,closedChunkSystem,tileMap,offsetPosition);
            }
            if (tileMap is not TileGridMap tileGridMap) {
                return;
            }
            TileHelper.tilePlaceTileEntityUpdate(placePosition, tileItem,tileGridMap);
            
        }

        public static void placeTileEntity(TileItem tileItem, ClosedChunkSystem closedChunkSystem,ITileMap tileMap, Vector2 offsetPosition) {
            Vector2Int chunkPosition = Global.getChunkFromWorld(offsetPosition);
            Vector2Int tileMapPosition = Global.getCellPositionFromWorld(offsetPosition);
            Vector2Int partitionPosition = Global.getPartitionFromWorld(offsetPosition)-chunkPosition*Global.PartitionsPerChunk;
            Vector2Int positionInChunk = tileMapPosition-chunkPosition*Global.ChunkSize;
            Vector2Int positionInPartition = positionInChunk-partitionPosition*Global.ChunkPartitionSize;
            ILoadedChunk chunk = closedChunkSystem.getChunk(chunkPosition);
            if (chunk == null) {
                Debug.LogError("Attempted to add TileEntity to null chunk. Chunk [" + chunkPosition.x + "," + chunkPosition.y + "]");
                return;
            }
            IChunkPartition partition = chunk.getPartition(partitionPosition);
            if (partition == null) {
                Debug.LogError("Attempted to add TileEntity to null partition. Chunk [" + chunkPosition.x + "," + chunkPosition.y + "], Partition:" + partitionPosition.x + "," + partitionPosition.y + "]");
                return;
            }
            
            TileEntityObject tileEntity = tileItem.tileEntity;
            if (tileEntity == null) {
                return;
            }
            ITileEntityInstance tileEntityInstance = TileEntityHelper.placeTileEntity(tileItem,positionInChunk,chunk,true);
            TileMapLayer layer = tileMap.getType().toLayer();
            partition.addTileEntity(layer,tileEntityInstance,positionInPartition);
            if (closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                conduitTileClosedChunkSystem.tileEntityPlaceUpdate(tileEntityInstance);
            }
        }

        

        private static bool placeConduit(ConduitItem conduitItem, Vector2 worldPosition, ITileMap tileMap, ConduitTileClosedChunkSystem closedChunkSystem)
        {
            Vector2Int placePosition = tileMap.worldToTileMapPosition(worldPosition);
            ConduitType conduitType = conduitItem.GetConduitType();
            IConduitSystemManager conduitSystemManager = closedChunkSystem.GetManager(conduitType);
            EntityPortType entityPortType = conduitSystemManager.GetPortTypeAtPosition(placePosition.x,placePosition.y);
            ITileEntityInstance tileEntity = conduitSystemManager.GetTileEntityAtPosition(placePosition.x,placePosition.y);
            int state = conduitSystemManager.GetNewState(placePosition,ConduitPlacementMode.Any,conduitItem.id);
            IConduit conduit = ConduitFactory.CreateNew(conduitItem,entityPortType,placePosition.x,placePosition.y,state,tileEntity);
            conduitSystemManager.SetConduit(placePosition.x,placePosition.y,conduit);
            
            tileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,conduitItem);
            return true;
        }

        private static bool placeFluid(FluidTileItem fluidTileItem, Vector2 worldPosition, ITileMap tileMap) {
            if (tileMap is not FluidTileMap fluidTileMap) {
                return false;
            }
            Vector2Int placePosition = Global.getCellPositionFromWorld(worldPosition);
            tileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,fluidTileItem);
            return true;
        }

        

        private static bool conduitPlacable(ConduitItem conduitItem, Vector2 worldPosition, ITileMap tileMap) {
            Vector2Int tileMapPosition = tileMap.worldToTileMapPosition(worldPosition);
            return !tileMap.hasTile(tileMapPosition);
        }

        private static bool fluidPlacable(FluidTileItem fluidTileItem, Vector2 worldPosition, ITileMap tileMap) {
            Vector2Int tileMapPosition = tileMap.worldToTileMapPosition(worldPosition);
            return !tileMap.hasTile(tileMapPosition);
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
        /**
        Snaps the given x,y onto the grid. 
        **/
        public static Vector2 snapGrid(float x, float y) {
        // a 32 pixel sprite takes up 1 x 1 area of space, 16 x 16 takes up 0.5 x 0.5. 
            return new Vector2(snap(x), snap(y));
        }

        public static float getAdjustedY(float y, float spriteSizeY) {
            return y + (Mathf.CeilToInt(spriteSizeY/2f)-1)/2f;
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
        private static bool raycastTileInBox(Vector2 position, int layers) {
            return Physics2D.BoxCast(position,new Vector2(0.48f,0.48f),0f,Vector2.zero,Mathf.Infinity,layers).collider != null;
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
        private static bool backgroundTileWithinRange(float minX, float maxX, float minY, float maxY, int layers) {
            for (float x = minX; x <= maxX; x += 1/2f) {
                for (float y = minY; y <= maxY; y += 1/2f) {
                    foreach (GridLocation location in Enum.GetValues(typeof(GridLocation))) {
                        if (!raycastBackground(new Vector2(x,y), location,  layers)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool raycastBackground(Vector2 position, GridLocation gridLocation, int layers) {
            Vector2 offset;
            switch (gridLocation) {
                case GridLocation.UpLeft:
                    offset = new Vector2(-0.25f, 0.25f);
                    break;
                case GridLocation.UpRight:
                    offset = new Vector2(0.25f, 0.25f);
                    break;
                case GridLocation.DownRight:
                    offset = new Vector2(0.25f, -0.25f);
                    break;
                case GridLocation.DownLeft:
                    offset = new Vector2(-0.25f, -0.25f);
                    break;
                case GridLocation.Center:
                    offset = Vector2.zero;
                    break;
                default: // never called
                    offset = Vector2.zero;
                    break;
            }
            if (Physics2D.Raycast(position+offset,Vector2.zero,Mathf.Infinity,layers).collider != null) {
                return true;
            }
            return false;
        }
    

        private static bool tileWithinParameter(float minX, float maxX, float minY, float maxY, int layers) {
            return (
                tileWithinRange(minX-0.5f,minX-0.5f,minY,maxY,layers) ||
                tileWithinRange(maxX+0.5f,maxX+0.5f,minY,maxY,layers) ||
                tileWithinRange(minX,maxX,minY-0.5f,minY-0.5f,layers) ||
                tileWithinRange(minX,maxX,maxY+0.5f,maxY+0.5f,layers));
        }

        private static bool tileWithinIntervalAreaRange(FloatIntervalVector floatIntervalVector, TileMapLayer layer) {
            return tileWithinRange(
                floatIntervalVector.X.LowerBound,
                floatIntervalVector.X.UpperBound,
                floatIntervalVector.Y.LowerBound,
                floatIntervalVector.Y.UpperBound,
                layer.toRaycastLayers()
            );
        }

        private static bool tileWithinIntervalParameter(FloatIntervalVector floatIntervalVector, TileMapLayer layer) {
            return tileWithinParameter(
                floatIntervalVector.X.LowerBound,
                floatIntervalVector.X.UpperBound,
                floatIntervalVector.Y.LowerBound,
                floatIntervalVector.Y.UpperBound,
                layer.toRaycastLayers()
            );
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
