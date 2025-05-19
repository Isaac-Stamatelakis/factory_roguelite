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
        public abstract IEnumerator GenerateBase(int seed, Vector2Int worldSize);
        public abstract string GetBaseId();
        public abstract IEnumerator GenerateGrid(int seed,Vector2Int size);
        public abstract int[][] GenerateGridInstant(int seed, Vector2Int size);
    }
}

