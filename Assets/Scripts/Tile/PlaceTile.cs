using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceTile {
    private static int tileBlockLayer = 1 << LayerMask.NameToLayer("TileBlock");
    private static int tileObjectLayer = 1 << LayerMask.NameToLayer("TileObject");
    private static int tileBackgroundLayer = 1 << LayerMask.NameToLayer("TileBackground");
    private static int chunkLayer = 1 << LayerMask.NameToLayer("Chunk");
    
    private static TileGridMap tileBlockMap;
    private static TileGridMap tileBackgroundMap;
    private static TileGridMap tileObjectMap;
    private static ConduitTileMap energyConduitMap;
    private static ConduitTileMap itemConduitMap;
    private static ConduitTileMap fluidConduitMap;
    private static ConduitTileMap signalConduitMap;
    public static bool inChunk(float x, float y) {
        return ChunkHelper.snapChunk(x,y) != null;
    }
    /**
    Conditions:
    i) no tileBlock within sprite size.
    ii) no tileObject within sprite size.
    iii) tileblock below, above, left, or right, or a tilebackground at the location.
    **/

    public static bool Place(IdData idData, Vector2 worldPlaceLocation) {
        if (idData is TileData) {
            string tileType = ((TileData) idData).tileType;
            switch (tileType) {
                case "TileBlock":
                    return PlaceTile.placeTileBlock(idData.id, worldPlaceLocation);
                case "TileBackground":
                    return PlaceTile.placeTileBackground(idData.id, worldPlaceLocation); 
                case "TileObject":
                    return PlaceTile.placeTileObject(idData.id, worldPlaceLocation);
            }
        } else if (idData is ConduitData) {
            string conduitType = ((ConduitData) idData).conduitType;
            return PlaceTile.placeConduit(idData.id,conduitType, worldPlaceLocation);
        }
        return false;
    }
    public static bool tileBlockPlacable(int id,float x, float y) { 
            
        FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(new Vector2(x,y),Global.getSpriteSize(id));
        if (tileWithinIntervalAreaRange(intervalVector,tileBlockLayer)) {
            return false;
        }
        if (tileWithinIntervalAreaRange(intervalVector,tileObjectLayer)) {
            return false;
        }
        if (GameObject.Find("Player").GetComponent<DevMode>().noPlaceLimit) {
            return true;
        } 
        if (!tileWithinIntervalParameter(intervalVector,tileBlockLayer) && 
            !tileWithinIntervalParameter(intervalVector,tileObjectLayer) &&
            !tileWithinIntervalParameter(intervalVector,tileBackgroundLayer)) {
            return false;
        }
        return true;
    }   

  
    /**
    Conditions:
    i) no tileBackground within sprite size.
    ii) tileBackground below, above, left, or right, or a tileblock at the location.
    **/
    public static bool tileBackgroundPlacable(int id,float x, float y) { 
        FloatIntervalVector intervalVector = TileHelper.getRealCoveredArea(new Vector2(x,y),Global.getSpriteSize(id));
        if (tileWithinRange(intervalVector.X.LowerBound,intervalVector.X.UpperBound,intervalVector.Y.LowerBound,intervalVector.Y.UpperBound,tileBackgroundLayer)) {
            return false;
        }
        if (GameObject.Find("Player").GetComponent<DevMode>().noPlaceLimit) {
            return true;
        } 
        if (!tileWithinParameter(intervalVector.X.LowerBound,intervalVector.X.UpperBound,intervalVector.Y.LowerBound,intervalVector.Y.UpperBound, tileBlockLayer) && 
            !tileWithinParameter(intervalVector.X.LowerBound,intervalVector.X.UpperBound,intervalVector.Y.LowerBound,intervalVector.Y.UpperBound, tileObjectLayer) &&
            !tileWithinParameter(intervalVector.X.LowerBound,intervalVector.X.UpperBound,intervalVector.Y.LowerBound,intervalVector.Y.UpperBound, tileBackgroundLayer) &&
            !tileWithinRange(intervalVector.X.LowerBound,intervalVector.X.UpperBound,intervalVector.Y.LowerBound,intervalVector.Y.UpperBound, tileBlockLayer)) {
            return false;
        }
        return true;
    }
    /**
    Is placable iff a tileBlock is placable
    **/ 
    public static bool tileObjectPlacable(int id,float x, float y) {
        return tileBlockPlacable(id, x, y);
        
    }
    /// <summary>
    /// Places a TileBlock at a given location if is a valid placement
    /// </summary>
    /// <param name = "id"> The id of the TileBlock to be placed </param>
    /// <param name = "x"> The x position to be placed at</param>
    /// <param name = "y"> The y position to be placed at </param>
    /// <returns>true if placed, false if not placed </returns>
    public static bool placeTileBlock(int id, Vector2 worldPlaceLocation) {
        
        if(!tileBlockPlacable(id,worldPlaceLocation.x,worldPlaceLocation.y)) {
            return false;
        }
        placeTile(id,worldPlaceLocation.x,worldPlaceLocation.y,"TileBlocks");
        return true;

    }
    /// <summary>
    /// Places a TileBackground at a given location if is a valid placement
    /// </summary>
    /// <param name = "id"> The id of the TileBlock to be placed </param>
    /// <param name = "x"> The x position to be placed at</param>
    /// <param name = "y"> The y position to be placed at </param>
    /// <returns>true if placed, false if not placed </returns>
    public static bool placeTileBackground(int id, Vector2 worldPlaceLocation) {
        if (!tileBackgroundPlacable(id,worldPlaceLocation.x,worldPlaceLocation.y)) {
            return false;
        }
        placeTile(id,worldPlaceLocation.x,worldPlaceLocation.y,"TileBackgrounds");
        return true;
    }
    /// <summary>
    /// Places a TileObject at a given location if is a valid placement
    /// </summary>
    /// <param name = "id"> The id of the TileBlock to be placed </param>
    /// <param name = "x"> The x position to be placed at</param>
    /// <param name = "y"> The y position to be placed at </param>
    /// <returns>true if placed, false if not placed </returns>
    public static bool placeTileObject(int id, Vector2 worldPlaceLocation) {
        if (!tileObjectPlacable(id,worldPlaceLocation.x,worldPlaceLocation.y)) {
            return false;
        }
        placeTile(id,worldPlaceLocation.x,worldPlaceLocation.y,"TileObjects");
        return true;
    }
    /// <summary>
    /// Places a Tile at a given location.
    /// </summary>
    /// <param name = "id"> The id of the TileBlock to be placed </param>
    /// <param name = "x"> The x position to be placed at</param>
    /// <param name = "y"> The y position to be placed at </param>
    /// <param name = "containerName"> The name of the GameObjectContainer which the tile is to be placed in </param>
    private static void placeTile(int id, float x, float y, string containerName) {
        Vector2Int placePosition = getPlacePosition(id,x,y);
        GameObject chunkGameObject = ChunkHelper.snapChunk(x,y);   
        if (chunkGameObject == null) {
            return;
        }
        GameObject tileContainer = Global.findChild(chunkGameObject.transform.parent.parent.transform,containerName);
        GameObject tileEntityContainer = Global.findChild(chunkGameObject.transform,"TileEntities");
        
        TileEntityFactory.createTileEntity(
            id,
            null,
            tileEntityContainer.transform,containerName,
            new Vector2Int(Global.modInt(placePosition.x,16),Global.modInt(placePosition.y,16))
        );
        tileContainer.GetComponent<TileGridMap>().placeTileAtLocation(placePosition.x,placePosition.y,id);
    }

    public static Vector2Int getPlacePosition(int id, float x, float y) {
        Vector2 spriteSize = Global.getSpriteSize(id);
        return new Vector2Int(snap(x),snap(y));
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
    /**
    returns true if there is a tile at the location
    **/
    public static bool raycastTileAtLocation(Vector2 position, int layer) {
        return Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer).collider != null;
    }
    /// <summary>
    /// raycasts a given position in a 0.5f, 0.5f box.
    /// </summary>
    public static bool raycastTileInBox(Vector2 position, int layer) {
        return Physics2D.BoxCast(position,new Vector2(0.48f,0.48f),0f,Vector2.zero,Mathf.Infinity,layer).collider != null;
    }

    /**
    returns true if there is a tile within the range, inclusive
    **/
    public static bool tileWithinRange(float minX, float maxX, float minY, float maxY, int layer) {
        
        for (float x = minX; x <= maxX; x += 1/2f) {
            for (float y = minY; y <= maxY; y += 1/2f) {
                if (raycastTileInBox(new Vector2(x,y), layer)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool tileWithinParameter(float minX, float maxX, float minY, float maxY, int layer) {
        return (
            tileWithinRange(minX-0.5f,minX-0.5f,minY,maxY,layer) ||
            tileWithinRange(maxX+0.5f,maxX+0.5f,minY,maxY,layer) ||
            tileWithinRange(minX,maxX,minY-0.5f,minY-0.5f,layer) ||
            tileWithinRange(minX,maxX,maxY+0.5f,maxY+0.5f,layer));
    }

    public static bool tileWithinIntervalAreaRange(FloatIntervalVector floatIntervalVector, int layer) {
        return tileWithinRange(floatIntervalVector.X.LowerBound,floatIntervalVector.X.UpperBound,floatIntervalVector.Y.LowerBound,floatIntervalVector.Y.UpperBound,layer);
    }

    public static bool tileWithinIntervalParameter(FloatIntervalVector floatIntervalVector, int layer) {
        return tileWithinParameter(floatIntervalVector.X.LowerBound,floatIntervalVector.X.UpperBound,floatIntervalVector.Y.LowerBound,floatIntervalVector.Y.UpperBound,layer);
    }

    public static bool placeConduit(int id, string conduitType, Vector2 placePosition) {
        if (conduitPlacable(id,conduitType,placePosition)) {
            ConduitTileMap conduitTileMap = GetConduitTileMap(placePosition, conduitType);
            Vector3Int tileMapPosition = conduitTileMap.mTileMap.WorldToCell(placePosition);
            conduitTileMap.placeTileAtLocation(tileMapPosition.x,tileMapPosition.y,id);
            
            return true;
        }
        return false;
    }

    public static bool conduitPlacable(int id, string conduitType, Vector2 placePosition) {
        ConduitTileMap conduitTileMap = GetConduitTileMap(placePosition, conduitType);
        
        if (conduitTileMap == null) {
            return false;
        }
        Vector3Int tileMapPosition = conduitTileMap.mTileMap.WorldToCell(placePosition);
        if (conduitTileMap.mTileMap.GetTile(new Vector3Int(tileMapPosition.x,tileMapPosition.y,0)) != null) {
            return false;
        }
        
        if (raycastTileAtLocation(placePosition,tileBlockLayer)) {
            return false;
        }
        return true;
    }

    private static ConduitTileMap GetConduitTileMap(Vector2 position, string conduitType) {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, chunkLayer);
        if (hit.collider == null) {
            return null;
        }
        Transform systemContainer = hit.transform.parent.parent;
        switch (conduitType) {
            case "energy":
                return Global.findChild(systemContainer,"EnergyConduits").GetComponent<ConduitTileMap>();
            case "item":
                return Global.findChild(systemContainer,"ItemConduits").GetComponent<ConduitTileMap>();
            case "fluid":
                return Global.findChild(systemContainer,"FluidConduits").GetComponent<ConduitTileMap>();
            case "signal":
                return Global.findChild(systemContainer,"SignalConduits").GetComponent<ConduitTileMap>();
        }
        return null;
    }
}
