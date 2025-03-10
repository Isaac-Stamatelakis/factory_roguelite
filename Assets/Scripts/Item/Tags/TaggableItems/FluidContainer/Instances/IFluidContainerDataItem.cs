using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;

namespace Items.Tags.FluidContainers {
    [CreateAssetMenu(fileName ="I~New Fluid Container Item",menuName="Item/Tagged Items/Fluid Container/Item")]
    public class IFluidContainerDataItem : PresetItemObject, IFluidContainerData
    {
        [SerializeField] public uint storage;
        [Header("Set size so fluid fits in container\nLeave at 0,0 for no sprite")]
        [SerializeField] public Vector2Int fluidSizeInSprite;
        [Header("Scale of Fluid Sprite in Item Entity Sprite Renderer")]
        public Vector2 WorldScale = new Vector2(0.75f, 1.6f);
        public Vector2Int GetFluidSpriteSize()
        {
            return fluidSizeInSprite;
        }

        public Vector2 GetWorldFluidSpriteScale()
        {
            return WorldScale;
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

