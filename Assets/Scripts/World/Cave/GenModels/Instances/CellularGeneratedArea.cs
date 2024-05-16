using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;
using Entities;
using LibNoise;
using LibNoise.Generator;

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
        [Header("Base Tile")]
        public TileItem tileItem;
        public int cellRadius;
        public int cellNeighboorCount;
        public int smoothIterations;
        [HideInInspector] public float fillPercent;
        [HideInInspector] public float frequency = 1f;
        [HideInInspector] public float lacunarity = 2f;
        [HideInInspector] public float persistence = 0.5f;
        [HideInInspector] public int octaveCount = 4;
        [HideInInspector] public QualityMode qualityMode = QualityMode.High;
        public RandomType randomType;

        public override SeralizedWorldData generateBase(int seed) {
            Debug.Log(randomType);
            int[,] grid = generateGrid(seed,getChunkSize()*Global.ChunkSize);
            grid = cellular_automaton(grid,getChunkSize()*Global.ChunkSize);
            return generateWorld(grid);
        }

        private int[,] generateNoiseField(Vector2Int size, int seed) {
            if (randomType == RandomType.Standard) {
                return getStandard(size);
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
            int[,] noiseField = new int[size.x,size.y];
            Noise2D noise2D = new Noise2D(size.x,size.y,moduleBase);
            noise2D.GeneratePlanar(0,size.x,0,size.y);
            float[,] noiseValues = noise2D.GetData();
            for (int x = 0; x < size.x; x ++) {
                for (int y = 0; y < size.y; y++) {
                    float r = noiseValues[x,y];
                    if (r < 0) {
                        noiseField[x, y] = 0;
                    } else {
                        noiseField[x, y] = 1;
                    }
                }
            }
            return noiseField;
        }
        private int[,] getStandard(Vector2Int size) {
            int[,] noiseField = new int[size.x,size.y];
            for (int x = 0; x < size.x; x ++) {
                for (int y = 0; y < size.y; y++) {
                    float r = UnityEngine.Random.Range(0f,1f);    
                    if (r < fillPercent) {
                        noiseField[x, y] = 0;
                    } else {
                        noiseField[x, y] = 1;
                    }
                }
            }
            return noiseField;
        }
        private int[,] cellular_automaton(int[,] grid,Vector2Int size) {
            for (int n = 0; n < smoothIterations; n ++) {
                int[,] tempGrid = new int[size.x, size.y];
                for (int x = 0; x < size.x; x ++) {
                    for (int y = 0; y < size.y; y ++) {
                        tempGrid[x,y] = grid[x,y];
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
                                if (tempGrid[xIndex,yIndex] == 1) {
                                    neighboors ++;
                                }
                            }
                        }
                        if (neighboors > cellNeighboorCount) {
                            grid[x,y] = 0;
                        } else {
                            grid[x,y] = 1;
                        }
                    }
                }
            } 
            return grid;
        }

        private SeralizedWorldData generateWorld(int[,] grid) {
            UnityEngine.Vector2Int caveSize = getChunkSize();
            int width = Global.ChunkSize *caveSize.x;
            int height = Global.ChunkSize*caveSize.y;
            SerializedBaseTileData baseTileData = new SerializedBaseTileData();
            baseTileData.ids = new string[width,height];
            baseTileData.sTileEntityOptions = new string[width,height];
            baseTileData.sTileOptions = new string[width,height];

            SerializedBackgroundTileData backgroundTileData = new SerializedBackgroundTileData();
            backgroundTileData.ids = new string[width,height];

            SeralizedFluidTileData fluidTileData = new SeralizedFluidTileData();
            fluidTileData.ids = new string[width,height];
            fluidTileData.fill = new int[width,height];
            for (int x = 0; x < width; x ++) {
                for (int y = 0; y < height; y ++) {
                    if (grid[x,y] == 1) {
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
            return worldTileData;
        }

        public override int[,] generateGrid(int seed, Vector2Int size)
        {
            UnityEngine.Random.InitState(seed);
            int[,] noiseField = generateNoiseField(size,seed);
            int[,] grid = cellular_automaton(noiseField,size);
            return grid;
        }

    }
}

