using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [Header("Defines the base structure of the area")]
        public GenerationModel generationModel;
        [Header("Distrubutes tiles around the area")]
        public AreaTileDistributor tileDistributor;
        [Header("Distrubutes structures around the area")]
        public AreaStructureDistributor structureDistributor;
        public SeralizedWorldData generate(int seed) {
            SeralizedWorldData worldTileData = generationModel.generateBase(seed);
            Vector2Int size = getChunkCaveSize()*Global.ChunkSize;
            //tileDistributor.distribute(worldTileData,seed,size.x,size.y);
            AreaGenerationHelper.SetNatureTileStates(worldTileData,size.x,size.y);
            //structureDistributor.distribute(worldTileData,seed);
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

