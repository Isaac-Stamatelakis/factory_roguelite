using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/CaveTeleporter")]
    public class CaveTeleporter : TileEntity, IRightClickableTileEntity
    {
        public GameObject ui;
        public void onRightClick()
        {
            GameObject instantiated = GameObject.Instantiate(ui);
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);
        }
    }
}

