using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc.Audio;
using UnityEngine.AddressableAssets;

namespace WorldModule.Caves {
    public interface IGeneratedArea {

    }
    [CreateAssetMenu(fileName ="New Cave",menuName="Generation/Cave")]
    public class Cave : ScriptableObject, IGeneratedArea
    {
        [SerializeField] private string id;
        public string Id {get => id;}
        [TextArea] [SerializeField] private string description;
        public string Description {get => description;}
        public AssetReference generationModel;
        public AssetReference[] tileGenerators;
        public AssetReference entityDistributor;
        public AssetReference structureDistributor;
        public List<AssetReference> songs;
        
    }

    public class CaveInstance: IGeneratedArea {
        private Cave cave;
        private CaveElements caveElements;

        public CaveInstance(Cave cave, CaveElements caveElements)
        {
            this.cave = cave;
            this.caveElements = caveElements;
        }

        public SeralizedWorldData generate(int seed) {
            UnityEngine.Random.InitState(seed);
            SeralizedWorldData worldTileData = caveElements.GenerationModel.generateBase(seed);
            Vector2Int size = getChunkCaveSize()*Global.ChunkSize;
            IntervalVector coveredArea = getChunkCoveredArea();
            Vector2Int bottomLeft = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound) * Global.ChunkSize;
            foreach (CaveTileGenerator generator in caveElements.TileGenerators) {
                generator.distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            if (caveElements.EntityDistributor != null) {
                caveElements.EntityDistributor.distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            AreaGenerationHelper.smoothNatureTiles(worldTileData,size.x,size.y);
            MusicTrackController.Instance.setSong(caveElements.Songs);
            return worldTileData;
        }
        public UnityEngine.Vector2Int getChunkCaveSize() {
            return caveElements.GenerationModel.getChunkSize();
        }
        public IntervalVector getChunkCoveredArea() {
            return caveElements.GenerationModel.getCoveredChunkArea();
        }
    }

    public static class CaveUtils {
        public static IEnumerable LoadCaveElements(Cave cave) {
            var entityHandle = cave.entityDistributor.LoadAssetAsync<Object>();
            var generationModelHandle = cave.generationModel.LoadAssetAsync<Object>();
            
            yield return entityHandle;
            yield return generationModelHandle;
            
        }
    }

    [System.Serializable] 
    public struct CaveElements {
        public GenerationModel GenerationModel;
        public List<CaveTileGenerator> TileGenerators;
        public CaveEntityDistributor EntityDistributor;
        public List<AudioClip> Songs;
    }

    

}

