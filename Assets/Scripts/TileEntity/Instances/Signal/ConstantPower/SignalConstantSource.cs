using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.Signal {
    [CreateAssetMenu(fileName = "E~Signal Source", menuName = "Tile Entity/Signal/Source")]
    public class SignalConstantSource : TileEntityObject
    {
        public ConduitPortLayout ConduitLayout;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalConstantSourceInstance(this,tilePosition,tileItem,chunk);
        }
    }

    public class SignalConstantSourceInstance : TileEntityInstance<SignalConstantSource>, ISignalConduitInteractable
    {
        private int signal;
        public SignalConstantSourceInstance(SignalConstantSource tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return true;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public void InsertSignal(bool signal, Vector2Int portPosition)
        {
            
        }
    }
}

