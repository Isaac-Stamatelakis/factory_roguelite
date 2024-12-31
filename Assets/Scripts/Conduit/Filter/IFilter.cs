using Item.Slot;

namespace Conduits.Ports {
    public interface IFilter {
        public bool Filter(ItemSlot itemSlot);
    }
}