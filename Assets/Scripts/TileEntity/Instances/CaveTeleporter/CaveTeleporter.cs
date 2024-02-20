using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/CaveTeleporter")]
    public class CaveTeleporter : TileEntity, IClickableTileEntity
    {
        public GameObject ui;
        public void onClick()
        {
            GameObject instantiated = GameObject.Instantiate(ui);
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);
        }
    }
}

