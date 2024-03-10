using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.ClosedChunkSystemModule;

namespace DimensionModule {

    public interface IMultipleSystemController {
        public ClosedChunkSystem getSystemFromWorldPosition(Vector2 position);
        public ClosedChunkSystem getSystemFromCellPositon(Vector2Int position);
    }

    public interface ISingleSystemController {
        public ClosedChunkSystem getSystem();
    }
    public abstract class DimController : MonoBehaviour
    {
        
    }
}


