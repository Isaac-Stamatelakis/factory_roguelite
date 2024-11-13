using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntityModule.Instances.Signal {
    public enum LogicGateType {
        And,
        Or,
        Xor
    }
    [CreateAssetMenu(fileName = "E~Signal Logic Gate", menuName = "Tile Entity/Signal/Gate")]
    public class SignalLogicGate : TileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public LogicGateType LogicGateType;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalLogicGateInstance(this,tilePosition,tileItem,chunk);
        }
    }

    public class SignalLogicGateInstance : TileEntityInstance<SignalLogicGate>, ISignalConduitInteractable
    {
        private bool topSignal;
        private bool botSignal;
        public SignalLogicGateInstance(SignalLogicGate tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public bool extractSignal(Vector2Int portPosition)
        {
            return isOpen();
        }

        private bool isOpen() {
            switch (tileEntity.LogicGateType) {
                case LogicGateType.Xor:
                    return (!topSignal && botSignal) || (topSignal && !botSignal);
                case LogicGateType.And:
                    return topSignal && botSignal;
                case LogicGateType.Or:
                    return topSignal || botSignal;
            }
            return false;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public void insertSignal(bool signal, Vector2Int portPosition)
        {
            if (portPosition.y > 0) {
                topSignal = signal;
            } else if (portPosition.y < 0) {
                botSignal = signal;
            }
        }
    }
}

