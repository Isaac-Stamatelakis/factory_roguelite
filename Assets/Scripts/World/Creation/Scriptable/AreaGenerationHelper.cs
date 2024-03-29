using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.IO;
using Tiles;
using ItemModule;

namespace WorldModule.Generation {
    public static class AreaGenerationHelper
    {
        public static void SetNatureTileStates(WorldTileData worldTileData, int width, int height) {
            SerializedBaseTileData serializedTileData = worldTileData.baseData;
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    string id = serializedTileData.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.getTileItem(id);
                    if (tileItem == null) {
                        continue;
                    }
                    if (tileItem.tile is not NatureTile natureTile) {
                        continue;
                    }
                    
                    if (y+1 >= height || y -1 < 0 || x + 1 >= width || x - 1 < 0) {
                        continue;
                    }
                    TileItem itemCopy = GameObject.Instantiate(tileItem);
                    TileOptionFactory.deserialize(serializedTileData.sTileOptions[x,y],itemCopy);
                    
                    
                    itemCopy.tileOptions.StaticOptions.hasStates = true;
                    itemCopy.tileOptions.StaticOptions.rotatable = true;
                    int state = 0;
                    int rotation = 0;
                    bool mirror = false;
                    string upID = serializedTileData.ids[x,y+1];
                    string downID = serializedTileData.ids[x,y-1];
                    string leftID = serializedTileData.ids[x-1,y];
                    string rightID = serializedTileData.ids[x+1,y];
                    
                    int nullCount = 0;
                    if (upID ==null) nullCount++;
                    if (downID ==null) nullCount++;
                    if (leftID ==null) nullCount++;
                    if (rightID ==null) nullCount++;

                    if (nullCount == 2) {
                        if (upID == null && leftID == null) {
                            state = natureTile.getRandomSlantState();
                            rotation = 0;
                        }
                        else if (upID == null && rightID == null) {
                            state = natureTile.getRandomSlantState();
                            mirror = true;
                        }
                        else if (downID == null && rightID == null) {
                            state = natureTile.getRandomSlantState();
                            rotation = 90;
                            mirror = true;
                        }
                        else if (downID == null && leftID == null) {
                            state = natureTile.getRandomSlantState();
                            rotation = 180;
                            mirror = true;
                        }
                    }
                    
                    if (state == 0 && upID == null && leftID != null && rightID != null) {
                        /*
                        string sLeftOptions = serializedTileData.sTileOptions[x-1,y];
                        TileItem leftItem = GameObject.Instantiate(itemRegistry.getTileItem(leftID));
                        
                        if (sLeftOptions != null) {
                            TileOptions leftOptions = TileOptionFactory.deserialize(sLeftOptions,leftItem);
                        }
                        serializedTileData.sTileOptions[x,y] = TileOptionFactory.serialize(itemCopy.tileOptions); 
                        */
                        //state = natureTile.getRandomNatureSlabState();
                    }
                    SerializedTileOptions sTileOptions = itemCopy.tileOptions.SerializedTileOptions;
                    sTileOptions.state = state;
                    sTileOptions.rotation = rotation;
                    sTileOptions.mirror = mirror;
                    itemCopy.tileOptions.SerializedTileOptions = sTileOptions;
                    serializedTileData.sTileOptions[x,y] = TileOptionFactory.serialize(itemCopy.tileOptions); 

                }
            }
        }
    }
}

