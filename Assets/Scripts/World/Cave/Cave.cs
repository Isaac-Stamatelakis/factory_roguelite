using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc.Audio;

namespace WorldModule.Caves {
    public interface IGeneratedArea {

    }
    [CreateAssetMenu(fileName ="New Cave",menuName="Generation/Cave")]
    public class Cave : ScriptableObject, IGeneratedArea
    {
        [SerializeField] private string id;
        public string Id {get => id;}
        [SerializeField] private string description;
        public string Description {get => description;}
        public GenerationModel generationModel;
        public CaveTileGenerator[] tileGenerators;
        public CaveEntityDistributor entityDistributor;
        public List<AudioClip> songs;
        public Sprite background;
        public SeralizedWorldData generate(int seed) {
            UnityEngine.Random.InitState(seed);
            SeralizedWorldData worldTileData = generationModel.generateBase(seed);
            Vector2Int size = getChunkCaveSize()*Global.ChunkSize;
            IntervalVector coveredArea = getChunkCoveredArea();
            Vector2Int bottomLeft = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound) * Global.ChunkSize;
            foreach (CaveTileGenerator generator in tileGenerators) {
                generator.distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            if (entityDistributor != null) {
                entityDistributor.distribute(worldTileData,size.x,size.y,bottomLeft);
            }
            AreaGenerationHelper.smoothNatureTiles(worldTileData,size.x,size.y);
            MusicTrackController.Instance.setSong(songs);
            return worldTileData;
        }
        public UnityEngine.Vector2Int getChunkCaveSize() {
            return generationModel.getChunkSize();
        }
        public IntervalVector getChunkCoveredArea() {
            return generationModel.getCoveredChunkArea();
        }
    }

    

}

