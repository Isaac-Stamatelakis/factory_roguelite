using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Tags.FluidContainers {
    public interface IFluidContainer : ITaggable
    {
        public int getStorage();
        public Vector2Int getFluidSpriteSize();
    }
}

