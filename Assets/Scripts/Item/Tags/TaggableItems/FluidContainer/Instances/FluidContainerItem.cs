using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Storage {
    public class FluidContainerItem : ItemObject, IFluidContainer
    {
        [SerializeField] public Sprite sprite;
        public override Sprite getSprite()
        {
            return sprite;
        }

        public void transferFluid(ItemSlot fluidItem)
        {
            throw new System.NotImplementedException();
        }
    }
}

