using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public interface IGenerationModel {
        public SeralizedWorldData generateBase(int seed);
    }
    
    public enum GenerationModelType {
        Cellular
    }
    public abstract class GenerationModel : ScriptableObject, IGenerationModel
    {
        public abstract SeralizedWorldData generateBase(int seed);
        [Header("X Interval in Chunks")]
        [SerializeField] public Vector2Int xInterval;
        [Header("Y Interval in Chunks")]
        [SerializeField] public  Vector2Int yInterval;
        public Vector2Int getChunkSize() {
            return new UnityEngine.Vector2Int(Mathf.Abs(xInterval.y - xInterval.x+1), Mathf.Abs(yInterval.y - yInterval.x+1));
        }
        public IntervalVector getCoveredChunkArea() {
            return new IntervalVector(new Interval<int>(xInterval.x,xInterval.y), new Interval<int>(yInterval.x,yInterval.y));
        }
    }
}

