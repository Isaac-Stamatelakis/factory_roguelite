using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags.FluidContainers {
    public interface IFluidContainer : ITaggableItem
    {
        public uint GetStorage();
        public Vector2Int GetFluidSpriteSize();
    }
}

