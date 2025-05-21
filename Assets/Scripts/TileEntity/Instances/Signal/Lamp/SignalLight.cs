using System.Collections;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Tiles;

namespace TileEntity.Instances.Signal {
    [CreateAssetMenu(fileName = "E~Signal Light", menuName = "Tile Entity/Signal/Light")]
    public class SignalLight : Torch
    {
        public ConduitPortLayout ConduitLayout;
        public int ActiveTicks = 12;

        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalLampInstance(this,tilePosition,tileItem,chunk);
        }
    }

    public class SignalLampInstance : TileEntityInstance<SignalLight>, ILoadableTileEntity, ISignalConduitInteractable, IOverrideSoftLoadTileEntity, IConduitPortTileEntity
    {
        private bool active;
        private GameObject lightObject;
        public SignalLampInstance(SignalLight tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return false;
        }
        
        public void InsertSignal(bool signal, Vector2Int portPosition)
        {
            if (signal == active) return;
            active = signal;
            if (active) {
                Load();
            } else
            {
                IChunkPartition partition = GetPartition();
                if (tileItem.tile is IStateTileSingle && partition.GetLoaded() && !partition.GetScheduledForUnloading())
                {
                    TileEntityUtils.stateSwitch(this,0); // set off
                }
                Unload();
            }
        }

        public void Load() {
            if (!active) {
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk || !GetPartition().GetLoaded()) {
                return;
            }
            if (!ReferenceEquals(lightObject,null)) return;
            
            
            lightObject = new GameObject
            {
                name = "Torch[" + positionInChunk.x + "," + positionInChunk.y + "]"
            };
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.intensity = TileEntityObject.intensity;
            Color color = TileEntityObject.color;
            color.a = 1;
            light.color = color;
            light.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
            light.pointLightOuterRadius=TileEntityObject.radius;
            light.falloffIntensity=TileEntityObject.falloff;
            lightObject.transform.position = (Vector2) positionInChunk/2 + TileEntityObject.positionInTile;
            lightObject.transform.SetParent(loadedChunk.GetTileEntityContainer(),false);
            IChunkPartition partition = GetPartition();
            if (tileItem.tile is IStateTileSingle stateTile &&  partition.GetLoaded() && !partition.GetScheduledForUnloading()) {
                TileEntityUtils.stateSwitch(this,1); // set on
            }
        }

        public void Unload()
        {
            if (!ReferenceEquals(lightObject, null))
            {
                GameObject.Destroy(lightObject);
                lightObject = null;
            }
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }
    }
}

