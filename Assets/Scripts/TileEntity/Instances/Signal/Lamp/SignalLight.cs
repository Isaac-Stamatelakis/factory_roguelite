using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Tiles;

namespace TileEntityModule.Instances.Signal {
    [CreateAssetMenu(fileName = "E~Signal Light", menuName = "Tile Entity/Signal/Light")]
    public class SignalLight : Torch
    {
        public ConduitPortLayout ConduitLayout;
        public int ActiveTicks = 12;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalLampInstance(this,tilePosition,tileItem,chunk);
        }
    }

    public class SignalLampInstance : TileEntityInstance<SignalLight>, ILoadableTileEntity, ISignalConduitInteractable
    {
        private bool active;
        private GameObject lightObject;
        public SignalLampInstance(SignalLight tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }

        public bool extractSignal(Vector2Int portPosition)
        {
            return false;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public void insertSignal(bool signal, Vector2Int portPosition)
        {
            active = signal;
            if (signal) {
                load();
            } else {
                unload();
            }
        }

        public void load() {
            if (!active) {
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            Color color = tileEntity.color;
            color.a = 1;
            if (lightObject == null) {
                lightObject = new GameObject();
                lightObject.name = "Torch[" + positionInChunk.x + "," + positionInChunk.y + "]"; 
                Light2D light = lightObject.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.intensity = tileEntity.intensity;
                light.color = color;
                light.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
                light.pointLightOuterRadius=tileEntity.radius;
                light.falloffIntensity=tileEntity.falloff;
                lightObject.transform.position = (Vector2) positionInChunk/2 + tileEntity.positionInTile;
                lightObject.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
                if (tileItem.tile is IStateTile stateTile) {
                    TileEntityHelper.stateSwitch(this,1); // set on
                }
            }
        }

        public void unload()
        {
            GameObject.Destroy(lightObject);
            if (tileItem.tile is IStateTile stateTile) {
                TileEntityHelper.stateSwitch(this,0); // set off
            }
        }
    }
}

