using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.SimonSays {
    public class SimonSaysColoredTileEntityInstance : TileEntityInstance<SimonSaysColoredTileEntityObject>, IRightClickableTileEntity
    {
        public SimonSaysColoredTileEntityInstance(SimonSaysColoredTileEntityObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        private SimonSaysControllerInstance controller;
        public SimonSaysControllerInstance Controller {set => controller = value;}
        public void onRightClick()
        {
            if (controller == null) {
                return;
            }
            controller.CoroutineController.showTileClick(this);
        }

        public void setColor(int color) {
            TileEntityHelper.stateSwitch(this,color);
        }
    }
}

