using Item.Slot;
using UnityEngine;

namespace TileEntity.Instances.Storage.Fluid
{
    public class FluidTankVisualManager : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        public void Initialize()
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        public void UpdateVisual(ItemSlot itemSlot, float fillRatio, Vector2 tileEntityPosition)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            spriteRenderer.sprite = itemSlot.itemObject.GetSprite();
            float height = fillRatio;
            transform.localScale = new Vector3(1f,height,1);
            transform.position = new Vector3(tileEntityPosition.x,tileEntityPosition.y-Global.TILE_SIZE+height/2,3f);
            //transform.position = new Vector3(tileEntityPosition.x,tileEntityPosition.y-0.25f+height/2,1.5f);
        }
    }
}
