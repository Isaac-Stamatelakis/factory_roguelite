using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Ladder")]
    public class Ladder : TileEntity, IClimableTileEntity
    {
        [SerializeField] private int speed;

        public int getSpeed()
        {
            return speed;
        }
    }

}
