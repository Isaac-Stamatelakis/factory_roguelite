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

        public void load() {
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to load torch in unloaded chunk");
                return;
            }
            Color color = TileEntityObject.color;
            color.a = 1;
            if (lightObject == null) {
                lightObject = new GameObject();
                lightObject.name = "Torch[" + positionInChunk.x + "," + positionInChunk.y + "]"; 
                Light2D light = lightObject.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.intensity = TileEntityObject.intensity;
                light.color = color;
                light.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
                light.pointLightOuterRadius=TileEntityObject.radius;
                light.falloffIntensity=TileEntityObject.falloff;
                lightObject.transform.position = (Vector2) positionInChunk/2 + TileEntityObject.positionInTile;
                lightObject.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
            }
        }

        public void unload()
        {
            if (lightObject != null) {
                GameObject.Destroy(lightObject);
            }
        }
    }
}

