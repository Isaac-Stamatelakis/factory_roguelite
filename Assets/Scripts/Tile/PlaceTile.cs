using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule;
using System;
using TileMapModule.Layer;
using TileMapModule.Type;
using TileMapModule.Conduit;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule.PartitionModule;
using ConduitModule;
using ConduitModule.Ports;
using ConduitModule.Systems;
using Tiles;

namespace TileMapModule.Place {
    public static class PlaceTile {
        private enum GridLocation {
            UpLeft,
            UpRight,
            DownRight,
            DownLeft,
            Center
        }
    
        public static bool PlaceFromCellPosition(ItemObject itemObject, Vector2Int cellPosition, ClosedChunkSystem closedChunkSystem, bool checkConditions = true) {
            // TODO: refactor this to be the base function rather than place from world. Lots of work though
            Vector2 worldPosition = new Vector2(cellPosition.x/2f,cellPosition.y/2f)+Vector2.one/4f;
            return PlaceFromWorldPosition(itemObject,worldPosition,closedChunkSystem,checkConditions);
        }

        public static void tilePlaceUpdate(Vector2Int position, TileMapType tileMapType) {

        }
        /**
        Conditions:
        i) no tileBlock within sprite size.
        ii) no tileObject within sprite size.
        iii) tileblock below, above, left, or right, or a tilebackground at the location.
        **/
        public static bool PlaceFromWorldPosition(ItemObject itemObject, Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem, bool checkConditions = true, TileEntity presetTileEntity = null) {
            if (itemObject is TileItem) {
                TileItem tileItem = (TileItem) itemObject;
                bool placable = true;
                if (checkConditions) {
                    placable = tilePlacable(tileItem,worldPlaceLocation);
                }
                if (placable) {
                    TileMapType tileMapType = tileItem.tileType.toTileMapType();
                    placeTile((TileItem) itemObject,worldPlaceLocation,closedChunkSystem.getTileMap(tileMapType),closedChunkSystem,presetTileEntity);
                    return true;
                }
            } else if (itemObject is ConduitItem) {
                if (closedChunkSystem is not ConduitTileClosedChunkSystem) {
                    return false;
                }
                bool placable = true;
                ConduitItem conduitItem = ((ConduitItem) itemObject);
                TileMapType tileMapType = conduitItem.getType().toTileMapType();
                ITileMap conduitMap = closedChunkSystem.getTileMap(tileMapType);
                if (conduitMap == null || conduitMap is not ConduitTileMap) {
                    return false;
                }
                if (checkConditions) {
                    placable = conduitPlacable(conduitItem,worldPlaceLocation,conduitMap);
                }
                
                if (placable) {
                    placeConduit(conduitItem,worldPlaceLocation,conduitMap,(ConduitTileClosedChunkSystem)closedChunkSystem);
                    return true;
                }
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
        private static void placeTile(TileItem tileItem, Vector2 worldPosition, ITileMap tileMap, ClosedChunkSystem closedChunkSystem, TileEntity presetTileEntity = null) {
            if (tileMap == null) {
                return;
            }
            if (tileItem.tile is IRestrictedTile restrictedTile) {
                int state = restrictedTile.getStateAtPosition(worldPosition,MousePositionFactory.getVerticalMousePosition(worldPosition),MousePositionFactory.getHorizontalMousePosition(worldPosition));
            }
            UnityEngine.Vector2Int placePosition = getPlacePosition(tileItem, worldPosition.x, worldPosition.y);
            tileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,tileItem);
            if (tileItem.tileEntity != null) {
                Vector2Int chunkPosition = Global.getChunkFromWorld(worldPosition);
                Vector2Int tileMapPosition = tileMap.worldToTileMapPosition(worldPosition);
                Vector2Int partitionPosition = Global.getPartitionFromWorld(worldPosition)-chunkPosition*Global.PartitionsPerChunk;
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
                TileEntity tileEntity = presetTileEntity;
                if (tileEntity == null) {
                    tileEntity = GameObject.Instantiate(tileItem.tileEntity);
                    tileEntity.initalize(positionInChunk, tileItem.tile, chunk);
                } 
                TileMapLayer layer = tileMap.getType().toLayer();
                partition.addTileEntity(layer,tileEntity,positionInPartition);
                if (closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                    if (tileEntity is IConduitInteractable) {
                        conduitTileClosedChunkSystem.tileEntityPlaceUpdate(tileEntity);
                    }
                }
            }
            if (tileMap is not TileGridMap tileGridMap) {
                return;
            }
            TileHelper.tileUpdate(placePosition, tileItem,tileGridMap);
            
        }

        private static bool placeConduit(ConduitItem conduitItem, Vector2 worldPosition, ITileMap tileMap, ConduitTileClosedChunkSystem closedChunkSystem) {
            Vector2Int placePosition = tileMap.worldToTileMapPosition(worldPosition);
            ConduitType conduitType = conduitItem.getType();
            IConduitSystemManager conduitSystemManager = closedChunkSystem.getManager(conduitType);
            EntityPortType entityPortType = conduitSystemManager.getPortTypeAtPosition(placePosition.x,placePosition.y);
            TileEntity tileEntity = conduitSystemManager.getTileEntityAtPosition(placePosition.x,placePosition.y);
            IConduit conduit = ConduitFactory.create(conduitItem,entityPortType,placePosition.x,placePosition.y,tileEntity);
            conduitSystemManager.setConduit(placePosition.x,placePosition.y,conduit);

            tileMap.placeNewTileAtLocation(placePosition.x,placePosition.y,conduitItem);
            return true;
        }

        

        private static bool conduitPlacable(ConduitItem conduitItem, Vector2 worldPosition, ITileMap tileMap) {
            Vector2Int tileMapPosition = tileMap.worldToTileMapPosition(worldPosition);
            return !tileMap.hasTile(tileMapPosition);
        }

        public static UnityEngine.Vector2Int getPlacePosition(ItemObject item, float x, float y) {
            return new UnityEngine.Vector2Int(snap(x), snap(y));
        }

        public static bool tileInDirection(Vector2 position, Direction direction, TileMapLayer layer) {
            float centeredX = (float)Math.Floor(position.x / 0.5f) * 0.5f + 0.25f;
            float centeredY = (float)Math.Floor(position.y / 0.5f) * 0.5f + 0.25f;
            Vector2 centered = new Vector2(centeredX,centeredY);
            switch (direction) {
                case Direction.Down:
                    return raycastTileInBox(centered+Vector2.down*0.5f,layer.toRaycastLayers());
                case Direction.Up:
                    return raycastTileInBox(centered+Vector2.up*0.5f,layer.toRaycastLayers());
                case Direction.Left:
                    return raycastTileInBox(centered+Vector2.left*0.5f,layer.toRaycastLayers());
                case Direction.Right:
                    return raycastTileInBox(centered+Vector2.right*0.5f,layer.toRaycastLayers());
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
