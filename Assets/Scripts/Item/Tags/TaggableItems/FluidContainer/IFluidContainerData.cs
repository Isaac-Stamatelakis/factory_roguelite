using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags.FluidContainers {
    public interface IFluidContainerData : ITaggableItem
    {
        public uint GetStorage();
        public Vector2Int GetFluidSpriteSize();
        public Vector2 GetWorldFluidSpriteScale();
    }
}

