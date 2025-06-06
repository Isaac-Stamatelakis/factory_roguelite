using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Items;
using UnityEngine;
using Misc.Audio;
using TileEntity;
using Tiles.TileMap.Interval;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using World.Cave.DecorationDistributor;
using World.Cave.Distributors.FluidDistributor;
using World.Cave.TileDistributor;
using World.Cave.TileDistributor.Ore;
using World.Cave.TileDistributor.Standard;
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
        public int ChunkWidth = 21;
        public int ChunkHeight = 21;
        public GenerationModel generationModel;
        public TileDistributorObject TileDistributorObject;
        public OreDistributionObject OreDistributionObject;
        public CaveEntityDistributor entityDistributor;
        public AreaStructureDistributor structureDistributor;
        public List<AudioClip> songs;
        public List<CaveDecoration> CaveDecorations;
        public List<FluidAreaDistribution> FluidAreaDistributions;
        public List<FluidPoolDistribution> FluidPoolDistributions;
        public CaveOptions CaveOptions;
        public List<EditorItemSlot> ResearchCost;
        
        public string GetId()
        {
            return name.Replace(" ", "_");
        }
        
        public UnityEngine.Vector2Int GetChunkCaveSize()
        {
            return new Vector2Int(ChunkWidth, ChunkHeight);
        }
        public IntervalVector GetChunkCoveredArea() {
            int xMin = -((ChunkWidth+1) / 2 - 1);
            int xMax = ChunkWidth / 2;
            int yMin = -((ChunkHeight+1) / 2 - 1);
            int yMax = ChunkHeight / 2;
            return new IntervalVector(new Interval<int>(xMin,xMax), new Interval<int>(yMin,yMax));
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

        public CaveInstance(CaveObject caveObject)
        {
            this.caveObject = caveObject;
        }

        public IEnumerator Generate(int seed) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            Vector2Int worldSize = new Vector2Int(caveObject.ChunkWidth, caveObject.ChunkHeight) * Global.CHUNK_SIZE;
            UnityEngine.Random.InitState(seed);
            IEnumerator worldTileDataEnumerator = caveObject.generationModel.GenerateBase(seed,worldSize);
            yield return worldTileDataEnumerator;
            SeralizedWorldData worldTileData =  worldTileDataEnumerator.Current as SeralizedWorldData;
            
            Vector2Int size = caveObject.GetChunkCaveSize()*Global.CHUNK_SIZE;
            IntervalVector coveredArea = caveObject.GetChunkCoveredArea();
            Vector2Int bottomLeft = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound) * Global.CHUNK_SIZE;
            stopwatch.Restart();

            if (caveObject.TileDistributorObject)
            {
                List<TileDistribution> tileDistributions = new List<TileDistribution>();
                foreach (StandardTileDistrubtion distributorObjectData in caveObject.TileDistributorObject.TileDistributions)
                {
                    if (distributorObjectData == null) continue;
                    List<TileDistributionFrequency> tileDistributionFrequencies = new List<TileDistributionFrequency>();
                    foreach (TileDistributionFrequency frequency in distributorObjectData.Tiles)
                    {
                        if (frequency.frequency == 0 || frequency.tileItem?.id == null) continue;
                        tileDistributionFrequencies.Add(frequency);
                    }
                    FrequencyTileAggregator frequencyTileAggregator = new FrequencyTileAggregator(tileDistributionFrequencies);
                    tileDistributions.Add(new TileDistribution(frequencyTileAggregator, distributorObjectData.TileDistributionData));
                }

                AreaTileDistributor areaTileDistributor = new AreaTileDistributor(tileDistributions, caveObject.generationModel.GetBaseId());
                areaTileDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            
            if (caveObject.structureDistributor)
            {
                caveObject.structureDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            
            if (caveObject.OreDistributionObject)
            {
                List<TileDistribution> tileDistributions = new List<TileDistribution>();
                foreach (OreDistribution oreDistribution in caveObject.OreDistributionObject.OreDistributions)
                {
                    OreTileAggregator oreTileAggregator = new OreTileAggregator(oreDistribution.Material,oreDistribution.SubDistrubtions);
                    tileDistributions.Add(new TileDistribution(oreTileAggregator,oreDistribution.TileDistributionData));

                }
                AreaTileDistributor oreDistributor = new AreaTileDistributor(tileDistributions,caveObject.generationModel.GetBaseId());
                oreDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            
            AreaGenerationHelper.SmoothNatureTiles(worldTileData,size.x,size.y);
            CaveDecorationDistributor caveDecorationDistributor = new CaveDecorationDistributor(caveObject.CaveDecorations);
            caveDecorationDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);

            FluidAreaDistributor fluidAreaDistributor = new FluidAreaDistributor(caveObject.FluidAreaDistributions);
            fluidAreaDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);
            
            FluidPoolDistributor fluidPoolDistributor = new FluidPoolDistributor(caveObject.FluidPoolDistributions);
            fluidPoolDistributor.Distribute(worldTileData,size.x,size.y,bottomLeft);

            caveObject.entityDistributor?.Distribute(worldTileData, size.x, size.y, bottomLeft);
            
            MusicTrackController.Instance.SetSong(caveObject.songs);
            yield return worldTileData;
        }
        
        
        
    }
}

