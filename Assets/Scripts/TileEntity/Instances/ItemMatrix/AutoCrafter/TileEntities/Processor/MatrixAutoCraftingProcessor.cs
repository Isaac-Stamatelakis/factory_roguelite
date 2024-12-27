using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Processor", menuName = "Tile Entity/Item Matrix/Crafting/Processor")]
    public class MatrixAutoCraftingProcessor : MatrixAutoCraftingChassis
    {
        public int Cores;
    }
}

