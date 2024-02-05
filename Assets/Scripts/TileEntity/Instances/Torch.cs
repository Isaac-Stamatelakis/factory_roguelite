using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "New Torch", menuName = "Tile Entity/Torch")]
public class Torch : TileEntity, ILoadableTileEntity
{
    public float intensity = 1;
    public int radius = 10;
    public float falloff = 0.7f;
    public Color color = Color.white;
    public Vector3 positionInTile;
    protected GameObject lightObject;
    public void load()
    {
        color.a = 1;
        if (lightObject == null) {
            lightObject = new GameObject();
            lightObject.name = "Torch[" + tilePosition.x + "," + tilePosition.y + "]"; 
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.intensity = intensity;
            light.color = color;
            light.pointLightOuterRadius=radius;
            light.falloffIntensity=falloff;
            lightObject.transform.position = getRealPosition()+positionInTile;
            lightObject.transform.SetParent(chunk.getTileEntityContainer());
        }
    }

    public void unload()
    {
        if (lightObject != null) {
            GameObject.Destroy(lightObject);
        }
    }
}
