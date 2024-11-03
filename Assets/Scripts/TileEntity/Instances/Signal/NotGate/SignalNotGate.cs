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
        private int signal;
        public SignalNotGateInstance(SignalNotGate tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public int extractSignal(Vector2Int portPosition)
        {
            if (signal > 0) {
                return 16;
            }
            return 0;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public void insertSignal(int signal, Vector2Int portPosition)
        {
            this.signal = signal;
        }
    }
}

