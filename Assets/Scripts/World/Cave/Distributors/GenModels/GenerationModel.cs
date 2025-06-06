using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public enum GenerationModelType {
        Cellular
    }
    public abstract class GenerationModel : ScriptableObject
    {
        [HideInInspector] [SerializeField] public CaveSimulationResults SimulationResults;
        
        public abstract IEnumerator GenerateBase(int seed, Vector2Int worldSize);
        public abstract string GetBaseId();
        public float FillEstimate => SimulationResults.EstimatedResultFillRatio;
        public float DecorationRatioEstimate => SimulationResults.EstimatedDecorationSpawnLocationRatio;
        public abstract IEnumerator GenerateGrid(int seed,Vector2Int size);
        public abstract int[][] GenerateGridInstant(int seed, Vector2Int size);
        
        [System.Serializable]
        public class CaveSimulationResults
        {
            public float EstimatedResultFillRatio;
            public float EstimatedDecorationSpawnLocationRatio;

            public CaveSimulationResults(float estimatedResultFillRatio, float estimatedDecorationSpawnLocationRatio)
            {
                EstimatedResultFillRatio = estimatedResultFillRatio;
                EstimatedDecorationSpawnLocationRatio = estimatedDecorationSpawnLocationRatio;
            }
            
            
        }

    }
}

