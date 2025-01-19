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

            BlockState?[][] states = new BlockState?[width][];
            for (int index = 0; index < width; index++)
            {
                states[index] = new BlockState?[height];
            }

            SerializedBaseTileData serializedTileData = worldTileData.baseData;
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    string id = serializedTileData.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.GetTileItem(id);
                    if (ReferenceEquals(tileItem,null)) {
                        continue;
                    }
                    if (tileItem.tile is not NatureTile natureTile) {
                        if (tileItem.tile is Tile tile) {
                            if (tile.colliderType == Tile.ColliderType.Sprite) {
                                states[x][y] = BlockState.Other;
                                continue;
                            }
                        }
                        states[x][y] = BlockState.Square;
                        continue;
                    }
                    
                    if (y+1 >= height || y -1 < 0 || x + 1 >= width || x - 1 < 0) {
                        continue;
                    }
                    SerializedTileOptions serializedTileOptions = TileOptionFactory.Deserialize(serializedTileData.sTileOptions[x,y],tileItem);
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
                            state = natureTile.GetRandomSlantState();
                            rotation = 0;
                        }
                        else if (upID == null && rightID == null) {
                            state = natureTile.GetRandomSlantState();
                            rotation = 3;
                        }
                        else if (downID == null && rightID == null) {
                            state = natureTile.GetRandomSlantState();
                            rotation = 2;
                        }
                        else if (downID == null && leftID == null) {
                            state = natureTile.GetRandomSlantState();
                            rotation = 1;
                        }
                    }
                    if (nullCount == 3) {
                        serializedTileData.ids[x,y] = null;
                        continue;
                    }
                    serializedTileOptions.state = state;
                    serializedTileOptions.rotation = rotation;
                    serializedTileOptions.mirror = mirror;
                    serializedTileData.sTileOptions[x,y] = TileOptionFactory.Serialize(serializedTileOptions); 
                    if (state > 0) {
                        states[x][y] = BlockState.Slant;
                    } else {
                        states[x][y] = BlockState.Square;
                    }
                }
            }
        }
        
    }
}

