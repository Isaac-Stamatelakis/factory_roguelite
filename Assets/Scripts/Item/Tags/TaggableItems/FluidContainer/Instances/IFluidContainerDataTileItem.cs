using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using TileEntity;

namespace Items.Tags.FluidContainers {
    [CreateAssetMenu(fileName ="I~New Fluid Container Tile Item",menuName="Item/Tagged Items/Fluid Container/Tile Item")]
    public class IFluidContainerDataTileItem : TileItem, IFluidContainerData
    {
        [Header("Set size so fluid fits in container\nLeave at 0,0 for no sprite")]
        [SerializeField] public Vector2Int fluidSizeInSprite;
        [Header("Scale of Fluid Sprite in Item Entity Sprite Renderer")]
        public Vector2 WorldScale;
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
            if (tileEntity == null) {
                Debug.LogError("cannot get storage from FluidContainerTileItem as tileEntity is null");
                return 0;
            }
            if (tileEntity is not FluidTank fluidTank) {
                Debug.LogError("Cannot get storage from FluidContainerTileItem " + name + " as tileEntity is not FluidTank");
                return 0;
            }
            return fluidTank.Tier.GetFluidStorage();
        }

        public List<ItemTag> getTags()
        {
            return new List<ItemTag>{ItemTag.FluidContainer};
        }
    }
}

