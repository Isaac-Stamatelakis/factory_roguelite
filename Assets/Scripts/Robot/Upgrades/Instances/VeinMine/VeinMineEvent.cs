using System.Collections.Generic;
using Item.Slot;
using Items;
using TileMaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Robot.Upgrades.Instances.VeinMine
{
    public interface IVeinMineEvent
    {
        public int Execute(Vector2Int initial, int veinMinePower);
        public List<ItemSlot> GetCollectedItems();
        public HashSet<Vector2Int> Preview(Vector2Int initial, int veinMinePower);
    }

    public class VeinMineItemCollector
    {
        public List<ItemSlot> ItemSlots = new List<ItemSlot>();
    }
    public abstract class VeinMineEvent<T> : IVeinMineEvent where T : IHitableTileMap
    {
        protected T hitableTileMap;
        protected Queue<Vector2Int> queue = new Queue<Vector2Int>();
        protected List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down,
        };
        
        protected VeinMineItemCollector veinMineItemCollector;
        protected VeinMineEvent(T hitableTileMap, bool dropItems)
        {
            this.hitableTileMap = hitableTileMap;
            if (!dropItems)
            {
                veinMineItemCollector = new VeinMineItemCollector();
            }
        }

        public int Execute(Vector2Int initial, int veinMinePower)
        {
            InitialExpand(initial);
            int breaks = 0;
            
            while (breaks < veinMinePower && queue.Count > 0)
            {
                bool broken = TryIterate();
                if (broken) breaks++;
            }
            
            return breaks;
        }
        

        private bool TryIterate()
        {
            Vector2Int current = queue.Dequeue();
            
            if (!hitableTileMap.hasTile(current)) return false;
            Expand(current);
            if (DevMode.Instance.instantBreak)
            {
                hitableTileMap.DeleteTile(new Vector2(current.x,current.y) * Global.TILE_SIZE);
            }
            else
            {
                bool drop = veinMineItemCollector is null;
                if (!drop)
                {
                    ItemObject itemObject = hitableTileMap.GetItemObject(current);
                    if (itemObject is TileItem tileItem)
                    {
                        List<ItemSlot> droppedItems = ItemSlotUtils.GetTileItemDrop(tileItem);
                        foreach (ItemSlot itemSlot in droppedItems)
                        {
                            ItemSlotUtils.AppendToInventory(veinMineItemCollector.ItemSlots, itemSlot, Global.MAX_SIZE);
                        }
                        
                    }
                    else
                    {
                        ItemSlot itemSlot = new ItemSlot(itemObject, 1, null);
                        ItemSlotUtils.AppendToInventory(veinMineItemCollector.ItemSlots, itemSlot, Global.MAX_SIZE);
                    }
                }
                
                hitableTileMap.BreakAndDropTile(current,drop);
            }
            
            return true;
        }

        public HashSet<Vector2Int> Preview(Vector2Int initial, int veinMinePower)
        {
            int safe = 0;
            InitialExpand(initial);
            HashSet<Vector2Int> broken = new HashSet<Vector2Int>();
            broken.Add(initial);
            int breaks = 0;
            while (breaks < veinMinePower && queue.Count > 0)
            {
                if (safe > 1000)
                {
                    Debug.Log("INF LOOP");
                    break;
                }

                Vector2Int current = queue.Dequeue();
                if (!broken.Add(current)) continue;
                breaks++;
                safe++;
                if (!hitableTileMap.hasTile(current)) continue;
                Expand(current);
            }
            return broken;
        }
        
        

        protected void Expand(Vector2Int current)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int newPosition = current + direction;
                if (!CanExpandTo(newPosition, current)) continue;
                queue.Enqueue(newPosition);
            }
        }

        public List<ItemSlot> GetCollectedItems()
        {
            return veinMineItemCollector.ItemSlots;
        }

        protected abstract void InitialExpand(Vector2Int initial);
        protected abstract bool CanExpandTo(Vector2Int position, Vector2Int origin);
    }
}