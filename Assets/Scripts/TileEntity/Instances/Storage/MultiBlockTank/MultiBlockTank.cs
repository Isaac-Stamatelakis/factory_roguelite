using System.Collections.Generic;
using Chunks;
using Chunks.Systems;
using Conduits.Ports;
using Item.Slot;
using Items;
using TileMaps.Layer;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileEntity.Instances.Storage.MultiBlockTank
{
    [CreateAssetMenu(fileName = "New MultiBlockTank", menuName = "Tile Entity/Storage/Fluid/MultiBlock")]
    public class MultiBlockTank : TileEntityObject
    {
        public TileItem TankTile;
        public uint SpacePerTank;
        public ConduitPortLayout ConduitLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MultiBlockTankInstance(this, tilePosition, tileItem, chunk);
        }
        
    }

    public class MultiBlockTankInstance : TileEntityInstance<MultiBlockTank>, IMultiBlockTileEntity, IItemConduitInteractable, ISerializableTileEntity, ILoadableTileEntity, IConduitPortTileEntity
    {
        private ItemSlot fluidSlot;
        private List<Vector2Int> tiles;
        private Dictionary<int, List<int>> fluidHeightMap;
        private int minY;
        private int maxY;
        public MultiBlockTankInstance(MultiBlockTank tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void AssembleMultiBlock()
        {
            tiles = TileEntityUtils.BFSTile(this, tileEntityObject.TankTile);
            minY = int.MaxValue;
            maxY = int.MinValue;
            fluidHeightMap = new Dictionary<int, List<int>>();
            foreach (Vector2Int tilePosition in tiles)
            {
                if (!fluidHeightMap.ContainsKey(tilePosition.y))
                {
                    fluidHeightMap[tilePosition.y] = new List<int>();
                }
                fluidHeightMap[tilePosition.y].Add(tilePosition.x);
                if (tilePosition.y < minY)
                {
                    minY = tilePosition.y;
                } else if (tilePosition.y > maxY)
                {
                    maxY = tilePosition.y;
                }
            }
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            if (state != ItemState.Fluid) return null;
            return fluidSlot;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (state != ItemState.Fluid) return;
            
            if (ItemSlotUtils.IsItemSlotNull(fluidSlot))
            {
                fluidSlot = new ItemSlot(toInsert.itemObject, toInsert.amount, null);
                toInsert.amount = 0;
                return;
            }
            if (!ItemSlotUtils.AreEqual(fluidSlot, toInsert)) return;
            uint size = (uint)tiles.Count;
            uint space = size * tileEntityObject.SpacePerTank;
            ItemSlotUtils.InsertIntoSlot(fluidSlot,toInsert,space);
            //Debug.Log(fluidSlot?.amount);
            DisplayFluid();
        }

        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(fluidSlot);
        }

        public void Unserialize(string data)
        {
            fluidSlot = ItemSlotFactory.DeserializeSlot(data);
        }

        public void Load()
        {
            DisplayFluid();
        }

        private void DisplayFluid()
        {
            if (chunk is not ILoadedChunk loadedChunk || fluidSlot.itemObject is not FluidTileItem fluidTileItem) return;

            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
            Tilemap tilemap = closedChunkSystem.GetTileEntityTileMap(TileEntityTileMapType.UnLitBack);
            uint remainingAmount = fluidSlot.amount;
            bool normalGravity = !fluidTileItem.fluidOptions.InvertedGravity;
            int y = normalGravity ? minY : maxY;
            while (true)
            {
                if (normalGravity)
                {
                    if (y > maxY) break;
                }
                else
                {
                    if (y < minY) break;
                }

                UpdateFluidRow(tilemap, fluidTileItem, y, ref remainingAmount);

                if (normalGravity)
                {
                    y++;
                }
                else
                {
                    y--;
                }
                
            }
        }

        private void UpdateFluidRow(Tilemap tilemap, FluidTileItem fluidTileItem, int y, ref uint remainingAmount)
        {
            List<int> xCoords = fluidHeightMap[y];
            uint spaceAtY = tileEntityObject.SpacePerTank * (uint)xCoords.Count;
            bool notFilledAtYLevel = remainingAmount < spaceAtY;
            float fillPercent = notFilledAtYLevel ? (float)remainingAmount / spaceAtY : 1;
            if (remainingAmount == 0)
            {
                foreach (int x in xCoords)
                {
                    tilemap.SetTile(new Vector3Int(x,y,0), null);
                }
                return;
            }
            if (notFilledAtYLevel)
            {
                remainingAmount = 0;
            }
            else
            {
                remainingAmount -= spaceAtY;
            }

            TileBase tile = fluidTileItem.GetTile(fillPercent);
            foreach (int x in xCoords)
            {
                tilemap.SetTile(new Vector3Int(x,y,0), tile);
            }
        }
        public void Unload()
        {
            if (chunk is not ILoadedChunk loadedChunk) return;
            
            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
            Tilemap tilemap = closedChunkSystem.GetTileEntityTileMap(TileEntityTileMapType.UnLitBack);
            for (int y = minY; y <= maxY; y++)
            {
                List<int> xCoords = fluidHeightMap[y];
                foreach (int x in xCoords)
                {
                    tilemap.SetTile(new Vector3Int(x,y,0), null);
                }
            }
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitLayout;
        }
    }
}
