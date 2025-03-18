using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Misc.Audio;
using TileEntity;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using World.Cave.DecorationDistributor;
using World.Cave.TileDistributor;
using World.Cave.TileDistributor.Ore;
using Debug = UnityEngine.Debug;

namespace WorldModule.Caves {
    public interface IGeneratedArea {

    }
    [CreateAssetMenu(fileName ="New Cave",menuName="Generation/Cave")]
    public class CaveObject : ScriptableObject, IGeneratedArea
    {
        public Tier tier;
        [TextArea] [SerializeField] private string description;
        public string Description {get => description;}
        public AssetReference generationModel;
        public TileDistributorObject TileDistributorObject;
        public OreDistributionObject OreDistributionObject;
        public AssetReference entityDistributor;
        public AssetReference structureDistributor;
        public List<AssetReference> songs;
        public List<CaveDecoration> CaveDecorations;
        
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
        private CaveObject caveObject;
        public CaveObject CaveObject => caveObject;
        private CaveElements caveElements;

        public CaveInstance(CaveObject caveObject, CaveElements caveElements)
        {
            this.caveObject = caveObject;
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
            caveElements.TileDistributor?.Distribute(worldTileData,size.x,size.y,bottomLeft);
           
            double tileDistributionTime = stopwatch.Elapsed.TotalSeconds;
            total += tileDistributionTime;
            double entityDistributionTime = 0;
            if (!ReferenceEquals(caveElements.StructureDistributor, null))
            {
                caveElements.StructureDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            if (caveElements.CaveEntityDistributor != null) {
                stopwatch.Restart();
                caveElements.CaveEntityDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
                entityDistributionTime = stopwatch.Elapsed.TotalSeconds;
                total += entityDistributionTime;
            }
            
            caveElements.OreDistributor?.Distribute(worldTileData,size.x,size.y,bottomLeft);
            
            AreaGenerationHelper.smoothNatureTiles(worldTileData,size.x,size.y);
            CaveDecorationDistributor caveDecorationDistributor = new CaveDecorationDistributor(caveObject.CaveDecorations);
            caveDecorationDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
            
            MusicTrackController.Instance.SetSong(caveElements.Songs);
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
    
    public struct CaveElements {
        public GenerationModel GenerationModel;
        public AreaTileDistributor TileDistributor;
        public List<AudioClip> Songs;
        public AreaStructureDistributor StructureDistributor;
        public AreaTileDistributor OreDistributor;
        public CaveEntityDistributor CaveEntityDistributor;
    }

    

}

