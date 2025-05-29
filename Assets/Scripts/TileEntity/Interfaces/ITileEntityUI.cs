using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Item.Slot;
using Recipe.Objects.Restrictions;
using RecipeModule;
using World.Cave.Registry;


namespace TileEntity {
    public interface ITileEntityUI {
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance);
    }
}