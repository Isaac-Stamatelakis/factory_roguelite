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
        private int topSignal;
        private int botSignal;
        public SignalLogicGateInstance(SignalLogicGate tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public int extractSignal(Vector2Int portPosition)
        {
            return isOpen() ? 16 : 0;
        }

        private bool isOpen() {
            switch (tileEntity.LogicGateType) {
                case LogicGateType.Xor:
                    return (topSignal > 0 && botSignal <= 0) || (topSignal <= 0 && botSignal < 0);
                case LogicGateType.And:
                    return topSignal > 0 && botSignal > 0;
                case LogicGateType.Or:
                    return topSignal > 0 || botSignal > 0;
            }
            return false;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public void insertSignal(int signal, Vector2Int portPosition)
        {
            if (portPosition.y > 0) {
                topSignal = signal;
            } else if (portPosition.y < 0) {
                botSignal = signal;
            }
        }
    }
}

