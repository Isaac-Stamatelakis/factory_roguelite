using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags.FluidContainers {
    public interface IFluidContainer : ITaggableItem
    {
        public int getStorage();
        public Vector2Int getFluidSpriteSize();
    }
}

