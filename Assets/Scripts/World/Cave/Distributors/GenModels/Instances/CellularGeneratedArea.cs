using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Chunks.IO;
using Entities;
using LibNoise;
using LibNoise.Generator;
using Tiles;
using Debug = System.Diagnostics.Debug;

namespace WorldModule.Caves {
    public enum RandomType {
        Standard,
        Perlin,
        Billow,
        Voronoi,
        RidgedMultifractal,
        Spheres,
        
    }
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Model/Cellular")]
    
    public class CellularGeneratedArea : GenerationModel
    {
        public TileItem tileItem;
        public int cellRadius = 2;
        public int cellNeighboorCount = 14;
        public int smoothIterations = 5;
        [HideInInspector] public float fillPercent = 0.42f;
        [HideInInspector] public float frequency = 1f;
        [HideInInspector] public float lacunarity = 2f;
        [HideInInspector] public float persistence = 0.5f;
        [HideInInspector] public int octaveCount = 4;
        [HideInInspector] public QualityMode qualityMode = QualityMode.High;
        public RandomType randomType;

        public override IEnumerator GenerateBase(int seed, Vector2Int worldSize)
        {
            IEnumerator gridIEnumerator = GenerateGrid(seed,worldSize);
            yield return gridIEnumerator;
            int[][] grid = gridIEnumerator.Current as int[][];
            
            //grid = cellular_automaton(grid,worldSize);
            IEnumerator worldIEnumerator = GenerateWorld(grid,worldSize);
            yield return worldIEnumerator;
            SeralizedWorldData worldData = worldIEnumerator.Current as SeralizedWorldData;
            yield return worldData;
        }

        public override string GetBaseId()
        {
            return tileItem?.id;
        }

        private int[][] GenerateNoiseField(Vector2Int size, int seed) {
            if (randomType == RandomType.Standard) {
                return GetStandard(size);
            }
            ModuleBase moduleBase = null;
            switch (randomType) {
                case RandomType.Perlin:
                    Perlin perlin = new Perlin();
                    perlin.Frequency = frequency;
                    perlin.Lacunarity = lacunarity;
                    perlin.Persistence = persistence;
                    perlin.OctaveCount = octaveCount;
                    perlin.Quality = qualityMode;
                    perlin.Seed = seed;
                    moduleBase = perlin;
                    break;
                case RandomType.Billow:
                    Billow billow = new Billow();
                    billow.Frequency = frequency;
                    billow.Lacunarity = lacunarity;
                    billow.Persistence = persistence;
                    billow.OctaveCount = octaveCount;
                    billow.Quality = qualityMode;
                    billow.Seed = seed;
                    moduleBase = billow;
                    break;
                case RandomType.RidgedMultifractal:
                    RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();
                    ridgedMultifractal.Frequency = frequency;
                    ridgedMultifractal.Lacunarity = lacunarity;
                    ridgedMultifractal.OctaveCount = octaveCount;
                    ridgedMultifractal.Quality = qualityMode;
                    ridgedMultifractal.Seed = seed;
                    moduleBase = ridgedMultifractal;
                    break;
                case RandomType.Spheres:
                    Spheres spheres = new Spheres();
                    spheres.Frequency = frequency;
                    moduleBase = spheres;
                    break;
                case RandomType.Voronoi:
                    Voronoi voronoi = new Voronoi();
                    voronoi.Frequency = frequency;
                    voronoi.Seed = seed;
                    moduleBase = voronoi;
                    break;
            }
            int[][] noiseField = new int[size.x][];
            for (int i = 0; i < size.x; i++)
            {
                noiseField[i] = new int[size.y];
            }
            Noise2D noise2D = new Noise2D(size.x,size.y,moduleBase);
            noise2D.GeneratePlanar(0,size.x,0,size.y);
            float[,] noiseValues = noise2D.GetData();
            for (int x = 0; x < size.x; x ++) {
                for (int y = 0; y < size.y; y++) {
                    float r = noiseValues[x,y];
                    if (r < 0) {
                        noiseField[x][y] = 0;
                    } else {
                        noiseField[x][y] = 1;
                    }
                }
            }
            return noiseField;
        }
        private int[][] GetStandard(Vector2Int size)
        {
            int[][] noiseField = new int[size.x][];
            for (int i = 0; i < size.x; i++)
            {
                noiseField[i] = new int[size.y];
            }
            for (int x = 0; x < size.x; x ++) {
                for (int y = 0; y < size.y; y++) {
                    float r = UnityEngine.Random.Range(0f,1f);
                    if (r < fillPercent) {
                        noiseField[x][y] = 0;
                    } else {
                        noiseField[x][y] = 1;
                    }
                }
            }
            return noiseField;
        }
        private IEnumerator CellularAutomatonSmooth(int[][] grid,Vector2Int size)
        {
            const int DELAY_COUNT = 10000;
            int iterationCount = 0;
            var delay = new WaitForFixedUpdate();
            for (int n = 0; n < smoothIterations; n ++) {
                int[][] tempGrid = new int[size.x][];
                for (int index = 0; index < size.x; index++)
                {
                    tempGrid[index] = new int[size.y];
                }
                for (int x = 0; x < size.x; x ++) {
                    for (int y = 0; y < size.y; y ++) {
                        tempGrid[x][y] = grid[x][y];
                    }
                }
                yield return delay;
                for (int x = 0; x < size.x; x ++) {
                    for (int y = 0; y < size.y; y++) {
                        int neighboors = 0;
                        iterationCount++;
                        if (iterationCount > DELAY_COUNT)
                        {
                            iterationCount = 0;
                            yield return delay;
                        }
                        for (int j = -cellRadius; j <= cellRadius; j ++) {
                            for (int k = -cellRadius; k <= cellRadius; k ++) {
                                if (j == 0 && k == 0) {
                                    continue;
                                }
                                int xIndex = x+j; 
                                int yIndex = y+k;
                                if (xIndex < 0  || xIndex >= size.x || yIndex < 0 || yIndex >= size.y) {
                                    neighboors ++;
                                    continue;
                                }
                                if (tempGrid[xIndex][yIndex] == 1) {
                                    neighboors ++;
                                }
                            }
                        }
                        if (neighboors > cellNeighboorCount) {
                            grid[x][y] = 0;
                        } else {
                            grid[x][y] = 1;
                        }
                    }
                }

                yield return delay;
            } 
            yield return grid;
        }
        

        private IEnumerator GenerateWorld(int[][] grid,Vector2Int caveSize) {
            int width = caveSize.x;
            int height = caveSize.y;
            SerializedBaseTileData baseTileData = new SerializedBaseTileData();
            baseTileData.ids = new string[width,height];
            baseTileData.sTileEntityOptions = new string[width,height];
            baseTileData.sTileOptions = new BaseTileData[width,height];

            SerializedBackgroundTileData backgroundTileData = new SerializedBackgroundTileData();
            backgroundTileData.ids = new string[width,height];

            SeralizedFluidTileData fluidTileData = new SeralizedFluidTileData();
            fluidTileData.ids = new string[width,height];
            fluidTileData.fill = new float[width,height];
            for (int x = 0; x < width; x ++) {
                for (int y = 0; y < height; y ++) {
                    if (grid[x][y] == 1) {
                        baseTileData.ids[x,y] = tileItem.id;
                    } 
                }
            }
            SeralizedWorldData worldTileData = new SeralizedWorldData(
                baseTileData: baseTileData, 
                backgroundTileData: backgroundTileData,
                entityData: new List<SeralizedEntityData>(),
                fluidTileData: fluidTileData
            );
            yield return worldTileData;
        }

        public override IEnumerator GenerateGrid(int seed, Vector2Int size)
        {
            UnityEngine.Random.InitState(seed);
            int[][] noiseField = GenerateNoiseField(size,seed);
            var gridIEnumerator = CellularAutomatonSmooth(noiseField, size);
            yield return gridIEnumerator;
            int[][] grid = gridIEnumerator.Current as int[][];
            yield return grid;
        }

        public override int[][] GenerateGridInstant(int seed, Vector2Int size)
        {
            UnityEngine.Random.InitState(seed);
            int[][] grid = GenerateNoiseField(size,seed);
            CelluarAutomation();
            return grid;
            int[][] CelluarAutomation() {
                for (int n = 0; n < smoothIterations; n ++) {
                    int[][] tempGrid = new int[size.x][];
                    for (int index = 0; index < size.x; index++)
                    {
                        tempGrid[index] = new int[size.y];
                    }
                    for (int x = 0; x < size.x; x ++) {
                        for (int y = 0; y < size.y; y ++) {
                            tempGrid[x][y] = grid[x][y];
                        }
                    }
                    for (int x = 0; x < size.x; x ++) {
                        for (int y = 0; y < size.y; y++) {
                            int neighboors = 0;
                            for (int j = -cellRadius; j <= cellRadius; j ++) {
                                for (int k = -cellRadius; k <= cellRadius; k ++) {
                                    if (j == 0 && k == 0) {
                                        continue;
                                    }
                                    int xIndex = x+j; 
                                    int yIndex = y+k;
                                    if (xIndex < 0  || xIndex >= size.x || yIndex < 0 || yIndex >= size.y) {
                                        neighboors ++;
                                        continue;
                                    }
                                    if (tempGrid[xIndex][yIndex] == 1) {
                                        neighboors ++;
                                    }
                                }
                            }
                            if (neighboors > cellNeighboorCount) {
                                grid[x][y] = 0;
                            } else {
                                grid[x][y] = 1;
                            }
                        }
                    }
                }
                return grid;
            }
        }
    }
}

