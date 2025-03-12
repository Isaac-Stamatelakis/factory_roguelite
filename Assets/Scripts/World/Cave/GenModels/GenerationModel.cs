using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public interface IGenerationModel {
        public SeralizedWorldData GenerateBase(int seed);
    }
    
    public enum GenerationModelType {
        Cellular
    }
    public abstract class GenerationModel : ScriptableObject, IGenerationModel
    {
        public abstract SeralizedWorldData GenerateBase(int seed);
        public abstract string GetBaseId();
        public abstract int[][] GenerateGrid(int seed,Vector2Int size);
        [Header("X Interval in Chunks")]
        [SerializeField] public Vector2Int xInterval = new Vector2Int(-10,10);
        [Header("Y Interval in Chunks")]
        [SerializeField] public  Vector2Int yInterval = new Vector2Int(-10,10);
        public Vector2Int GetChunkSize() {
            return new UnityEngine.Vector2Int(Mathf.Abs(xInterval.y - xInterval.x+1), Mathf.Abs(yInterval.y - yInterval.x+1));
        }
        public IntervalVector GetCoveredChunkArea() {
            return new IntervalVector(new Interval<int>(xInterval.x,xInterval.y), new Interval<int>(yInterval.x,yInterval.y));
        }
    }
}

