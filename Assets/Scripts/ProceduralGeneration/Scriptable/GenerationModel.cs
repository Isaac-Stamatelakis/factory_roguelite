using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Generation {
    public interface IGenerationModel {
        public WorldTileData generateBase(int seed);
    }
    
    public abstract class GenerationModel : ScriptableObject, IGenerationModel
    {
        public abstract WorldTileData generateBase(int seed);
    }
}

