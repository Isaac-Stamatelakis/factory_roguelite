using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.ClosedChunkSystemModule;

namespace DimensionModule {
    public abstract class DimController : MonoBehaviour
    {
        public abstract ClosedChunkSystem getSystem(Vector2 position);
    }
}


