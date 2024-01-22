using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CaveTest : MonoBehaviour {
    void Update() {
        if (reset) {
            reset = !reset;
        }
        GenerateMap();
    }
    [SerializeField]
    bool reset;
    [SerializeField]
    public int width;
    [SerializeField]
    public int height;
    private int seed;
    [SerializeField]
    
    [Range(0,100)]
    int fillPercent = 55;
    [SerializeField]
    int iterations = 5;
    int[,] drawMap;

    public int[,] GenerateMap() {
		int[,] map = new int[width,height];
		RandomFillMap(map);

		for (int i = 0; i < iterations; i ++) {
			SmoothMap(map);
		}
        drawMap = map;
        return map;
	}


	void RandomFillMap(int[,] map) {
		System.Random pseudoRandom = new System.Random();

		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				if (x == 0 || x == width-1 || y == 0 || y == height -1) {
					map[x,y] = 1;
				}
				else {
					map[x,y] = (pseudoRandom.Next(0,100) < fillPercent)? 1: 0;
				}
			}
		}
	}
    void SmoothMap(int[,] map) {
        int[,] tempGrid = new int[width, height];
        Array.Copy(map, tempGrid, map.Length);
		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				int neighbourWallTiles = GetSurroundingWallCount(map,x,y);

				if (neighbourWallTiles > 4)
					tempGrid[x,y] = 1;
				else if (neighbourWallTiles < 4)
					tempGrid[x,y] = 0;

			}
		}
        Array.Copy(tempGrid, map, map.Length);
	}
    int GetSurroundingWallCount(int[,] map, int gridX, int gridY) {
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
					if (neighbourX != gridX || neighbourY != gridY) {
						wallCount += map[neighbourX,neighbourY];
					}
				}
				else {
					wallCount ++;
				}
			}
		}

		return wallCount;
	}
    
    void OnDrawGizmos() {
		if (drawMap != null) {
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					Gizmos.color = (drawMap[x,y] == 1)?Color.black:Color.white;
					Vector3 pos = new Vector3(-width/2 + x + .5f,0, -height/2 + y+.5f);
					Gizmos.DrawCube(pos,Vector3.one);
				}
			}
		}
	}
    /*
    /// Creates cool line patterns
     private int[,] cool_cellular_automaton(int[,] grid, int count) {
        for (int n = 0; n < count; n ++) {
            for (int x = 0; x < width; x ++) {
                int[,] tempGrid=(int[,])grid.Clone();
                for (int y = 0; y < height; y++) {
                    int neighboors = 0;
                    for (int j = -1; j <= 1; j ++) {
                        for (int k = -1; k <= 1; k ++) {
                            if (j == 0 && k == 0) {
                                continue;
                            }
                            int xIndex = x+j; int yIndex = y+k;
                            if (xIndex < 0  || xIndex >= width || yIndex < 0 || yIndex >= height) {
                                neighboors ++;
                                continue;
                            }
                            if (tempGrid[xIndex,yIndex] == 1) {
                                    neighboors ++;
                            }
                            
                        }
                    }
                    if (neighboors > 4) {
                        grid[x,y] = 0;
                    } else {
                        grid[x,y] = 1;
                    }
                }
            }
        }
        return grid;
    }
     */
}
