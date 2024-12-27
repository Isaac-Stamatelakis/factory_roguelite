using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;
using Tiles;
using UnityEngine.Tilemaps;
using Items;

namespace WorldModule.Caves {
    public static class AreaGenerationHelper
    {
        private enum BlockState {
            Slab,
            Square,
            Other,
            Slant,
        }
        public static void smoothNatureTiles(SeralizedWorldData worldTileData, int width, int height) {

            BlockState?[,] states = new BlockState?[width,height];
            SerializedBaseTileData serializedTileData = worldTileData.baseData;
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    string id = serializedTileData.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.GetTileItem(id);
                    if (tileItem == null) {
                        continue;
                    }
                    if (tileItem.tile is not NatureTile natureTile) {
                        if (tileItem.tile is Tile tile) {
                            if (tile.colliderType == Tile.ColliderType.Sprite) {
                                states[x,y] = BlockState.Other;
                                continue;
                            }
                        }
                        states[x,y] = BlockState.Square;
                        continue;
                    }
                    
                    if (y+1 >= height || y -1 < 0 || x + 1 >= width || x - 1 < 0) {
                        continue;
                    }
                    TileItem itemCopy = GameObject.Instantiate(tileItem);
                    TileOptionFactory.deserialize(serializedTileData.sTileOptions[x,y],itemCopy);
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
                            /*
                            rotation = 0;
                            */
                        }
                        else if (upID == null && rightID == null) {
                            state = natureTile.getRandomSlantState();
                            rotation = 3;
                            /*
                            mirror = true;
                            */
                        }
                        else if (downID == null && rightID == null) {
                            state = natureTile.getRandomSlantState();
                            rotation = 2;
                            /*
                            rotation = 90;
                            mirror = true;
                            */
                        }
                        else if (downID == null && leftID == null) {
                            state = natureTile.getRandomSlantState();
                            rotation = 1;
                            /*
                            rotation = 180;
                            mirror = true;
                            */
                        }
                    }
                    if (nullCount == 3) {
                        serializedTileData.ids[x,y] = null;
                        continue;
                    }
                    /*
                    if (nullCount == 3) {
                        if (upID != null) {
                            state = natureTile.getRandomNatureSlabState();
                            rotation = 180;
                        }
                        else if (downID != null) {
                            state = natureTile.getRandomNatureSlabState();
                            rotation = 0;
                        }
                        else if (leftID != null) {
                            state = natureTile.getRandomNatureSlabState();
                            rotation = 270;
                        }
                        else if (rightID != null) {
                            state = natureTile.getRandomNatureSlabState();
                            rotation = 90;
                        }
                    }
                    */
                    SerializedTileOptions sTileOptions = itemCopy.tileOptions.SerializedTileOptions;
                    sTileOptions.state = state;
                    sTileOptions.rotation = rotation;
                    sTileOptions.mirror = mirror;
                    itemCopy.tileOptions.SerializedTileOptions = sTileOptions;
                    serializedTileData.sTileOptions[x,y] = TileOptionFactory.serialize(itemCopy.tileOptions); 
                    if (state > 0) {
                        states[x,y] = BlockState.Slant;
                    } else {
                        states[x,y] = BlockState.Square;
                    }
                }
            }
            /*
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    BlockState? state = states[x,y];
                    if (state == null) {
                        continue;   
                    }
                    if (state == BlockState.Slant) {
                        continue;
                    }
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
                    BlockState? up = states[x,y+1];
                    BlockState? down = states[x,y-1];
                    BlockState? left = states[x-1,y];
                    BlockState? right = states[x+1,y];
                    if (up == null && left != null && right != null && left != BlockState.Slant && right != BlockState.Slant) {
                        TileItem copy = GameObject.Instantiate(tileItem);
                        SerializedTileOptions sTileOptions = copy.tileOptions.SerializedTileOptions;
                        sTileOptions.state = natureTile.getRandomNatureSlabState();
                        sTileOptions.rotation = 0;
                        sTileOptions.mirror = false;
                        copy.tileOptions.SerializedTileOptions = sTileOptions;
                        serializedTileData.sTileOptions[x,y] = TileOptionFactory.serialize(copy.tileOptions); 
                        states[x,y] = BlockState.Slab;
                    } 

                }
            }
            */
        }
        
    }
}

