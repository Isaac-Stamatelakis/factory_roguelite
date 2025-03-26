using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps;
using Chunks.Partitions;
using UnityEngine.Tilemaps;
using Items;
using System.Linq;
using Chunks;
using Chunks.Systems;
using TileMaps.Layer;
using Tiles.Fluid.Simulation;

namespace Fluids {
    public class FluidWorldTileMap : AbstractIWorldTileMap<FluidTileItem>, ITileMapListener
    {
        public void Awake()
        {
            simulator = new FluidTileMapSimulator(this);
        }

        private FluidTileMapSimulator simulator;
        public const float MAX_FILL = 1f;
        public override bool HitTile(Vector2 position, bool dropItem)
        {
            return false;
            // Cannot hit fluid tiles
        }

        public override ItemObject GetItemObject(Vector2Int position)
        {
            // Isn't required
            return null;
        }

        public override bool BreakAndDropTile(Vector2Int position, bool dropItem)
        {
            return false;
        }

        public void AddChunk(ILoadedChunk loadedChunk)
        {
            FluidCell[][] fluidCells = new FluidCell[Global.CHUNK_SIZE][];
            for (int index = 0; index < Global.CHUNK_SIZE; index++)
            {
                fluidCells[index] = new FluidCell[Global.CHUNK_SIZE];
            }

            for (int px = 0; px < Global.PARTITIONS_PER_CHUNK; px++)
            {
                for (int py = 0; py < Global.PARTITIONS_PER_CHUNK; py++)
                {
                    IChunkPartition partition = loadedChunk.GetPartition(new Vector2Int(px, py));
                    partition.AddFluidDataToChunk(fluidCells);
                }
            }

            simulator.AddChunk(loadedChunk.GetPosition(), fluidCells);
        }

        public void RemoveChunk(Vector2Int position)
        {
            simulator.RemoveChunk(position);
        }

        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            return Global.getCellPositionFromWorld(position);
        }

        protected override void SetTile(int x, int y, FluidTileItem item)
        {
            if (ReferenceEquals(item,null)) {
                tilemap.SetTile(new Vector3Int(x,y,0),null);
                return;
            }
            Vector2Int position = new Vector2Int(x,y);

            FluidCell fluidCell = simulator.GetFluidCell(position);

            DisplayTile(x, y, item, fluidCell.Liquid);
        }

        public void DisplayTile(int x, int y, FluidTileItem fluidTileItem, float fill)
        {
            int tileIndex = (int)(FluidTileItem.FLUID_TILE_ARRAY_SIZE * fill);
            Tile tile = fluidTileItem.getTile(tileIndex);
            tilemap.SetTile(new Vector3Int(x,y,0),tile);
        }
        
        public void DisplayTile(FluidCell fluidCell)
        {
            var fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(fluidCell.FluidId);
            if (!fluidTileItem)
            {
                tilemap.SetTile(new Vector3Int(fluidCell.Position.x,fluidCell.Position.y,0),null);
                return;
            }
            DisplayTile(fluidCell.Position.x,fluidCell.Position.y,fluidTileItem,fluidCell.Liquid);
        }
        
        protected override void WriteTile(IChunkPartition partition, Vector2Int positionInPartition, FluidTileItem item)
        {
            PartitionFluidData partitionFluidData = partition.GetFluidData();
            partitionFluidData.ids[positionInPartition.x,positionInPartition.y] = item.id;
            partitionFluidData.fill[positionInPartition.x,positionInPartition.y] = MAX_FILL;
            Vector2Int cellPosition = partition.GetRealPosition() * Global.CHUNK_PARTITION_SIZE + positionInPartition;
            FluidCell fluidCell = new FluidCell(item.id,MAX_FILL,FluidFlowRestriction.NoRestriction,cellPosition);
            simulator.AddFluidCell(fluidCell);
        }

        public void TileUpdate(Vector2Int position)
        {
            ILoadedChunkSystem chunkSystem = closedChunkSystem;
            var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(position);
            FluidCell fluidCell = partition.GetFluidCell(positionInPartition);
            simulator.AddFluidCell(fluidCell);
            simulator.UnsettleNeighbors(position);
        }

        public void FixedUpdate()
        {
            simulator.Simulate();
        }
    }
}

