using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntityModule.Instances.Signal {
    [CreateAssetMenu(fileName = "E~Signal Logic Gate", menuName = "Tile Entity/Signal/NotGate")]
    public class SignalNotGate : TileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalNotGateInstance(this,tilePosition,tileItem,chunk);
        }
    }

    public class SignalNotGateInstance : TileEntityInstance<SignalNotGate>, ISignalConduitInteractable
    {
        bool signal;
        public SignalNotGateInstance(SignalNotGate tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public bool extractSignal(Vector2Int portPosition)
        {
            return !signal;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public void insertSignal(bool signal, Vector2Int portPosition)
        {
            this.signal = signal;
        }
    }
}

