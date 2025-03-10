using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Misc.Audio;
using TileEntity;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;

namespace WorldModule.Caves {
    public interface IGeneratedArea {

    }
    [CreateAssetMenu(fileName ="New Cave",menuName="Generation/Cave")]
    public class Cave : ScriptableObject, IGeneratedArea
    {
        public Tier tier;
        [TextArea] [SerializeField] private string description;
        public string Description {get => description;}
        public AssetReference generationModel;
        public AssetReference[] tileGenerators;
        public AssetReference entityDistributor;
        public AssetReference structureDistributor;
        public List<AssetReference> songs;
        public CaveOptions CaveOptions;
        
        public string GetId()
        {
            return name.Replace(" ", "_");
        }
    }

    [System.Serializable]
    public class CaveOptions
    {
        public Color LightColor = new Color(1, 1, 1, 1);
        [Range(0, 2)] public float LightIntensity = 0.05f; 
        public Color OutlineColor = new Color(1, 1, 1, 1);
        public Color ParticleColor;
    }

    public class CaveInstance: IGeneratedArea {
        private Cave cave;
        public Cave Cave => cave;
        private CaveElements caveElements;

        public CaveInstance(Cave cave, CaveElements caveElements)
        {
            this.cave = cave;
            this.caveElements = caveElements;
        }

        public SeralizedWorldData Generate(int seed) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double total = 0;
            UnityEngine.Random.InitState(seed);
            SeralizedWorldData worldTileData = caveElements.GenerationModel.GenerateBase(seed);
            
            double modelTime = stopwatch.Elapsed.TotalSeconds;
            
            total += modelTime;
            
            Vector2Int size = getChunkCaveSize()*Global.CHUNK_SIZE;
            IntervalVector coveredArea = getChunkCoveredArea();
            Vector2Int bottomLeft = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound) * Global.CHUNK_SIZE;
            stopwatch.Restart();
            foreach (CaveTileGenerator generator in caveElements.TileGenerators) {
                generator.distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            double tileDistributionTime = stopwatch.Elapsed.TotalSeconds;
            total += tileDistributionTime;
            double entityDistributionTime = 0;
            if (!ReferenceEquals(caveElements.StructureDistributor, null))
            {
                caveElements.StructureDistributor.distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            if (!ReferenceEquals(caveElements.EntityDistributor,null)) {
                stopwatch.Restart();
                caveElements.EntityDistributor.distribute(worldTileData,size.x,size.y,bottomLeft);
                entityDistributionTime = stopwatch.Elapsed.TotalSeconds;
                total += entityDistributionTime;
            }
            
            AreaGenerationHelper.smoothNatureTiles(worldTileData,size.x,size.y);
            MusicTrackController.Instance.setSong(caveElements.Songs);
            stopwatch.Stop();
            Debug.Log($"Cave generated completed in {total:F2} seconds. Model Generation of {caveElements.GenerationModel.GetType().Name} Time: {modelTime:F2} seconds." +
                      $"Tile Distribution Time: {tileDistributionTime:F2} seconds. Entity Distribution Time: {entityDistributionTime:F2} seconds.");
            return worldTileData;
        }
        public UnityEngine.Vector2Int getChunkCaveSize() {
            return caveElements.GenerationModel.GetChunkSize();
        }
        public IntervalVector getChunkCoveredArea() {
            return caveElements.GenerationModel.GetCoveredChunkArea();
        }
    }


    [System.Serializable] 
    public struct CaveElements {
        public GenerationModel GenerationModel;
        public List<CaveTileGenerator> TileGenerators;
        public CaveEntityDistributor EntityDistributor;
        public List<AudioClip> Songs;
        public AreaStructureDistributor StructureDistributor;
    }

    

}

