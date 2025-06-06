using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.Signal {
    [CreateAssetMenu(fileName = "E~Signal Logic Gate", menuName = "Tile Entity/Signal/NotGate")]
    public class SignalNotGate : TileEntityObject
    {
        public ConduitPortLayout ConduitLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalNotGateInstance(this,tilePosition,tileItem,chunk);
        }
    }

    public class SignalNotGateInstance : TileEntityInstance<SignalNotGate>, ISignalConduitInteractable, IConduitPortTileEntity
    {
        bool signal;
        public SignalNotGateInstance(SignalNotGate tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return !signal;
        }

        public void InsertSignal(bool signal, Vector2Int portPosition)
        {
            this.signal = signal;
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }
    }
}

