using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Fluid Tank",menuName="Tile Entity/Fluid Tank")]
    public class FluidTank : TileEntity
    {
        [SerializeField] public Tier tier;
        public int getStorage() {
            return tier.getFluidStorage();
        }
    }
}

