using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.SimonSays {
    [CreateAssetMenu(fileName = "E~New Simon Says Controller", menuName = "Tile Entity/SimonSays/ColoredTile")]
    public class SimonSaysColoredTileEntity : TileEntity, IClickableTileEntity
    {
        private SimonSaysController controller;
        public SimonSaysController Controller {set => controller = value;}
        public void onClick()
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