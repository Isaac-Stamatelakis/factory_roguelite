using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.SimonSays {
    public class SimonSaysColoredTileEntityInstance : TileEntityInstance<SimonSaysColoredTileEntityObject>, IConditionalRightClickableTileEntity
    {
        public SimonSaysColoredTileEntityInstance(SimonSaysColoredTileEntityObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        private SimonSaysControllerInstance controller;
        public SimonSaysControllerInstance Controller {set => controller = value;}
        public void OnRightClick()
        {
            controller?.CoroutineController.ShowTileClick(this);
        }

        public bool CanRightClick()
        {
            if (ReferenceEquals(controller, null)) return false;
            return !(controller.PlayingSequence || controller.Restarting);
        }

        public void setColor(int color) {
            TileEntityUtils.stateSwitch(this,color);
        }
    }
}

