using Item.Slot;
using UnityEngine.Tilemaps;

namespace Item.ItemObjects.Interfaces
{
    public interface IPlacableItem
    {
        public TileBase GetTile();
    }
}
