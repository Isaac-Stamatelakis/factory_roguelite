using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags.FluidContainers {
    [CreateAssetMenu(fileName ="I~New Fluid Container Item",menuName="Item/Tagged Items/Fluid Container/Item")]
    public class FluidContainerItem : PresetItemObject, IFluidContainer
    {
        [SerializeField] public uint storage;
        [Header("Set size so fluid fits in container\nLeave at 0,0 for no sprite")]
        [SerializeField] public Vector2Int fluidSizeInSprite;

        public Vector2Int GetFluidSpriteSize()
        {
            return fluidSizeInSprite;
        }
        public uint GetStorage()
        {
            return storage;
        }

        public List<ItemTag> getTags()
        {
            return new List<ItemTag>{ItemTag.FluidContainer};
        }
    }
}

