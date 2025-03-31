using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TileEntity.Instances {
    public class TorchInstance : TileEntityInstance<Torch>, ILoadableTileEntity
    {
        protected GameObject lightObject;

        public TorchInstance(Torch tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void Load() {
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to load torch in unloaded chunk");
                return;
            }
            
            if (!ReferenceEquals(lightObject, null)) return;
            Color color = TileEntityObject.color;
            color.a = 1;
            lightObject = TileEntityUtils.SpawnLightObject(color, tileEntityObject.intensity, tileEntityObject.radius,
                tileEntityObject.falloff,
                (Vector2)positionInChunk / 2 + TileEntityObject.positionInTile, loadedChunk.GetTileEntityContainer());
        }
        public void Unload()
        {
            if (ReferenceEquals(lightObject, null)) return;
            GameObject.Destroy(lightObject);
            lightObject = null;
        }
    }
}

