using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public interface IGenerationModel {
        public SeralizedWorldData GenerateBase(int seed,Vector2Int worldSize);
    }
    
    public enum GenerationModelType {
        Cellular
    }
    public abstract class GenerationModel : ScriptableObject, IGenerationModel
    {
        public abstract SeralizedWorldData GenerateBase(int seed, Vector2Int worldSize);
        public abstract string GetBaseId();
        public abstract int[][] GenerateGrid(int seed,Vector2Int size);
    }
}

