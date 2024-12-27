using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Memory", menuName = "Tile Entity/Item Matrix/Crafting/Memory")]
    public class MatrixAutoCraftingMemory : MatrixAutoCraftingChassis
    {
        public int Memory;
    }
}

