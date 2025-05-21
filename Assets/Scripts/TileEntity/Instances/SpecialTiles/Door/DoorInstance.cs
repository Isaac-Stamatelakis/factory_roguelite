using System;
using System.Collections;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Conduits.Ports;
using Tiles;
using UnityEngine;

namespace TileEntity.Instances {
    
    public class DoorInstance : TileEntityInstance<Door>, IRightClickableTileEntity, IConditionalRightClickableTileEntity, ILockUnInteractableRightClickTileEntity, IOverrideSoftLoadTileEntity, ISignalConduitInteractable
    {
        private bool signalActive;
        public DoorInstance(Door tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public bool CanRightClick()
        {
            return !signalActive;
        }

        private int GetStateSwitchCount()
        {
            if (tileItem.tile is MousePositionStateTileSingleStateDoorTile)
            {
                return 2;
            }

            // TODO
            return 0;
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return false;
        }

        private bool IsCurrentlyOpen(int state)
        {
            if (tileItem.tile is MousePositionStateTileSingleStateDoorTile)
            {
                return state > 1;
            }

            return false;
        }

        public void InsertSignal(bool active, Vector2Int portPosition)
        {
            if (active == signalActive) return;
            signalActive = active;
            IChunkPartition chunkPartition = GetPartition();
            Vector2Int positionInPartition = GetPositionInPartition();
            
            BaseTileData baseTileData = chunkPartition.GetBaseData(positionInPartition);
            int state = baseTileData.state;
            bool open = IsCurrentlyOpen(state);
            if (!(active && !open) && !(!active && open)) return;
            
            int stateSwitch = GetStateSwitchCount();
            TileEntityUtils.stateIterate(this,stateSwitch);
        }

        public void OnRightClick()
        {
            if (signalActive) return;
            int stateSwitch = GetStateSwitchCount();
            TileEntityUtils.stateIterate(this,stateSwitch);
        }
    }
}

