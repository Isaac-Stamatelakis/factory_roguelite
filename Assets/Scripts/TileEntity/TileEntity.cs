using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;

namespace TileEntityModule {
    public abstract class TileEntity : ScriptableObject
    {
        protected Vector2Int tilePosition;
        protected IChunk chunk;
        public void initalize(Vector2Int tilePosition, IChunk chunk) {
            this.chunk = chunk;
            this.tilePosition = tilePosition;
        }

        public Vector2Int getPositionInChunk() {
            return tilePosition;
        }

        public Vector2 getWorldPosition() {
            return (tilePosition + chunk.getPosition() * Global.ChunkSize)/2;
        }
    }


} // end