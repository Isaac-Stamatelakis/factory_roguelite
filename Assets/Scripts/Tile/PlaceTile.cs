using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule;
using System;



public class PlaceTile {
    private enum GridLocation {
        UpLeft,
        UpRight,
        DownRight,
        DownLeft,
        Center
    }
    /**
    Conditions:
    i) no tileBlock within sprite size.
    ii) no tileObject within sprite size.
    iii) tileblock below, above, left, or right, or a tilebackground at the location.
    **/

    public static bool Place(ItemObject itemObject, Vector2 worldPlaceLocation, ClosedChunkSystem closedChunkSystem) {
        if (itemObject is TileItem) {
            TileItem tileItem = (TileItem) itemObject;
            if (tilePlacable(tileItem,worldPlaceLocation)) {
                TileMapType tileMapType = TileMapTypeFactory.tileToMapType(tileItem.tileType);
                placeTile((TileItem) itemObject,worldPlaceLocation,closedChunkSystem.getTileMap(tileMapType),closedChunkSystem);
                return true;
            }
        } else if (itemObject is ConduitItem) {
            ConduitItem conduitItem = ((ConduitItem) itemObject);
            TileMapType tileMapType = TileMapTypeFactory.tileToMapType(conduitItem.getType());
            TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(tileMapType);
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
        }
        return false;
    }
    private static bool tilePlacable(TileItem tileItem,Vector2 worldPlaceLocation) {
        TileMapType tileMapType = TileMapTypeFactory.tileToMapType(tileItem.tileType);
        TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(tileMapType);
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
            TileMapType tileMapType = TileMapTypeFactory.tileToMapType(conduitItem.getType());
            TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(tileMapType);
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
        }
        return false;
    }
    public static Vector3Int getItemPlacePosition(ItemObject itemObject, Vector2 position) {
        if (itemObject is TileItem) {
            TileItem tileItem = (TileItem) itemObject;
            return (Vector3Int)PlaceTile.getPlacePosition(tileItem,position.x,position.y);
        } else if (itemObject is ConduitItem) {

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
            TileMapTypeFactory.getLayersInTileMapLayer(TileMapLayer.Background)
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
                TileMapTypeFactory.getLayersInTileMapLayer(TileMapLayer.Base)
            ) && 
            !tileWithinParameter(
                intervalVector.X.LowerBound,
                intervalVector.X.UpperBound,
                intervalVector.Y.LowerBound,
                intervalVector.Y.UpperBound,
                TileMapTypeFactory.getLayersInTileMapLayer(TileMapLayer.Background)
            ) &&
            !tileWithinRange(
                intervalVector.X.LowerBound,
                intervalVector.X.UpperBound,
                intervalVector.Y.LowerBound,
                intervalVector.Y.UpperBound, 
                TileMapTypeFactory.getLayersInTileMapLayer(TileMapLayer.Base)
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
    private static void placeTile(TileItem tileItem, Vector2 worldPosition, ITileMap tileMap, ClosedChunkSystem closedChunkSystem) {
        if (tileMap == null) {
            return;
        }
        UnityEngine.Vector2Int placePosition = getPlacePosition(tileItem, worldPosition.x, worldPosition.y);
        if (tileItem.tileEntity != null) {
            Vector2Int chunkPosition = Global.getChunk(worldPosition);
            Vector2Int tileMapPosition = tileMap.worldToTileMapPosition(worldPosition);
            Vector2Int partitionPosition = Global.getPartition(worldPosition)-chunkPosition*Global.PartitionsPerChunk;
            Vector2Int positionInChunk = tileMapPosition-chunkPosition*Global.ChunkSize;
            Vector2Int positionInPartition = positionInChunk-partitionPosition*Global.ChunkPartitionSize;
            IChunk chunk = closedChunkSystem.getChunk(chunkPosition);
            if (chunk == null) {
                Debug.LogError("Attempted to add TileEntity to null chunk. Chunk [" + chunkPosition.x + "," + chunkPosition.y + "]");
                return;
            }
            IChunkPartition partition = chunk.getPartition(partitionPosition);
            if (partition == null) {
                Debug.LogError("Attempted to add TileEntity to null partition. Chunk [" + chunkPosition.x + "," + chunkPosition.y + "], Partition:" + partitionPosition.x + "," + partitionPosition.y + "]");
                return;
            }
            TileEntity tileEntity = GameObject.Instantiate(tileItem.tileEntity);
            tileEntity.initalize(positionInChunk, chunk);
            TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(tileMap.getType());
            partition.addTileEntity(layer,tileEntity,positionInPartition);
        }
        
        TileData tileData = new TileData(
            tileItem,
            tileItem.getOptions()
        );
        tileMap.placeTileAtLocation(placePosition.x,placePosition.y,tileData);
    }
    public static UnityEngine.Vector2Int getPlacePosition(TileItem tileItem, float x, float y) {
        Vector2 spriteSize = Global.getSpriteSize(tileItem.getSprite());
        return new UnityEngine.Vector2Int(snap(x), snap(y));
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
        /*
        if (Physics2D.BoxCast(position+offset,new Vector2(0.01f,0.01f),0f,Vector2.zero,Mathf.Infinity,layers).collider != null) {
            return true;
        }
        */
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
        int layers = TileMapTypeFactory.getLayersInTileMapLayer(layer);
        return tileWithinRange(
            floatIntervalVector.X.LowerBound,
            floatIntervalVector.X.UpperBound,
            floatIntervalVector.Y.LowerBound,
            floatIntervalVector.Y.UpperBound,
            layers
        );
    }

    private static bool tileWithinIntervalParameter(FloatIntervalVector floatIntervalVector, TileMapLayer layer) {
        int layers = TileMapTypeFactory.getLayersInTileMapLayer(layer);
        return tileWithinParameter(
            floatIntervalVector.X.LowerBound,
            floatIntervalVector.X.UpperBound,
            floatIntervalVector.Y.LowerBound,
            floatIntervalVector.Y.UpperBound,
            layers
        );
    }

    private static bool placeConduit(ConduitItem conduitItem, Vector2 placePosition) {
        if (conduitPlacable(conduitItem,placePosition)) {
            ConduitTileMap conduitTileMap = GetConduitTileMap(placePosition, conduitItem.getType());
            Vector3Int tileMapPosition = conduitTileMap.mTileMap.WorldToCell(placePosition);
            ConduitData conduitData = new ConduitData(
                conduitItem
            );
            conduitTileMap.placeTileAtLocation(tileMapPosition.x,tileMapPosition.y,conduitData);
            
            return true;
        }
        return false;
    }

    private static bool conduitPlacable(ConduitItem conduitItem, Vector2 placePosition) {
        ConduitTileMap conduitTileMap = GetConduitTileMap(placePosition, conduitItem.getType());
        
        if (conduitTileMap == null) {
            return false;
        }
        Vector3Int tileMapPosition = conduitTileMap.mTileMap.WorldToCell(placePosition);
        if (conduitTileMap.mTileMap.GetTile(new Vector3Int(tileMapPosition.x,tileMapPosition.y,0)) != null) {
            return false;
        }
        /*
        if (raycastTileAtLocation(placePosition,tileBlockLayer)) {
            return false;
        }
        */
        return true;
    }

    private static ConduitTileMap GetConduitTileMap(Vector2 position, ConduitType type) {
        /*
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, chunkLayer); 

        if (hit.collider == null) {
            return null;
        }
        Transform systemContainer = hit.transform.parent.parent;
        switch (type) {
            case ConduitType.Energy:
                return Global.findChild(systemContainer,"EnergyConduits").GetComponent<ConduitTileMap>();
            case ConduitType.Item:
                return Global.findChild(systemContainer,"ItemConduits").GetComponent<ConduitTileMap>();
            case ConduitType.Fluid:
                return Global.findChild(systemContainer,"FluidConduits").GetComponent<ConduitTileMap>();
            case ConduitType.Signal:
                return Global.findChild(systemContainer,"SignalConduits").GetComponent<ConduitTileMap>();
        }
        return null;
        */
        return null;
    }
}
