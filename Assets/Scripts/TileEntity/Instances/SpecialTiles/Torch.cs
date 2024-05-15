using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Chunks;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "New Torch", menuName = "Tile Entity/Torch")]
    public class Torch : TileEntity, ILoadableTileEntity
    {
        public float intensity = 1;
        public int radius = 10;
        public float falloff = 0.7f;
        public Color color = Color.white;
        public Vector2 positionInTile;
        protected GameObject lightObject;
        public void load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to load torch in unloaded chunk");
                return;
            }
            color.a = 1;
            if (lightObject == null) {
                lightObject = new GameObject();
                lightObject.name = "Torch[" + positionInChunk.x + "," + positionInChunk.y + "]"; 
                Light2D light = lightObject.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.intensity = intensity;
                light.color = color;
                light.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
                light.pointLightOuterRadius=radius;
                light.falloffIntensity=falloff;
                lightObject.transform.position = (Vector2) positionInChunk/2 + positionInTile;
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