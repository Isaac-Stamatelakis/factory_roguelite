using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags.FluidContainers {
    [CreateAssetMenu(fileName ="I~New Fluid Container Item",menuName="Item/Tagged Items/Fluid Container/Item")]
    public class FluidContainerItem : PresetItemObject, IFluidContainer
    {
        [SerializeField] public int storage;
        [Header("Set size so fluid fits in container\nLeave at 0,0 for no sprite")]
        [SerializeField] public Vector2Int fluidSizeInSprite;

        public Vector2Int getFluidSpriteSize()
        {
            return fluidSizeInSprite;
        }
        public int getStorage()
        {
            return storage;
        }

        public List<ItemTag> getTags()
        {
            return new List<ItemTag>{ItemTag.FluidContainer};
        }
    }
}

