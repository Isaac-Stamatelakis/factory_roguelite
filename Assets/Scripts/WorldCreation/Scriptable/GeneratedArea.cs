using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Generation {
    public interface IGeneratedArea {

    }
    [CreateAssetMenu(fileName ="New Area",menuName="Generation/Area")]
    public class GeneratedArea : ScriptableObject, IGeneratedArea
    {
        [Header("Defines the base structure of the area")]
        public GenerationModel generationModel;
        [Header("Distrubutes tiles around the area")]
        public AreaTileDistributor tileDistributor;
        [Header("Distrubutes structures around the area")]
        public AreaStructureDistributor structureDistributor;
        public WorldTileData generate(int seed) {
            WorldTileData worldTileData = generationModel.generateBase(seed);
            Vector2Int size = getChunkCaveSize()*Global.ChunkSize;
            tileDistributor.distribute(worldTileData,seed,size.x,size.y);
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

