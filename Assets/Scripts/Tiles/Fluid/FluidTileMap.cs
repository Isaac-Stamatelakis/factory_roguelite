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

namespace Fluids {
    public class FluidWorldTileMap : AbstractIWorldTileMap<FluidTileItem>, ITileMapListener
    {
        private HashSet<Vector2Int> refreshedPositions = new HashSet<Vector2Int>(1024);
        List<Vector2Int> toRefresh = new List<Vector2Int>
        {
            Vector2Int.zero,
            Vector2Int.left,
            Vector2Int.right,
        };
        const float EPSILON = 0.005f;
        private const float MAX_FILL = 1f;
        private uint ticks;
        private Dictionary<uint, HashSet<Vector2Int>> tickFluidUpdates = new Dictionary<uint, HashSet<Vector2Int>>(); 
        private Dictionary<Vector2Int, PartitionFluidData[][]> dualChunkFluidData = new Dictionary<Vector2Int, PartitionFluidData[][]>();

        public override void hitTile(Vector2 position)
        {
            // Cannot hit fluid tiles
        }

        public void AddChunk(ILoadedChunk loadedChunk)
        {
            var data = new PartitionFluidData[Global.PARTITIONS_PER_CHUNK][];
            for (int index = 0; index < Global.PARTITIONS_PER_CHUNK; index++)
            {
                data[index] = new PartitionFluidData[Global.PARTITIONS_PER_CHUNK];
            }

            for (int px = 0; px < Global.PARTITIONS_PER_CHUNK; px++)
            {
                for (int py = 0; py < Global.PARTITIONS_PER_CHUNK; py++)
                {
                    IChunkPartition partition = loadedChunk.GetPartition(new Vector2Int(px, py));
                    PartitionFluidData fluidData = partition.GetFluidData();
                    data[px][py] = fluidData;
                }
            }

            dualChunkFluidData[loadedChunk.GetPosition()] = data;
        }

        public void RemoveChunk(Vector2Int position)
        {
            if (!dualChunkFluidData.ContainsKey(position)) return;
            dualChunkFluidData.Remove(position);
            const int CHUNK_SIZE = Global.CHUNK_PARTITION_SIZE * Global.PARTITIONS_PER_CHUNK;
            
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    int idx = x + position.x * CHUNK_SIZE;
                    int idy = y + position.y * CHUNK_SIZE;
                    SetTile(idx, idy, null);
                }
            }
        }

        protected override Vector2Int GetHitTilePosition(Vector2 position)
        {
            return Global.getCellPositionFromWorld(position);
        }

        protected override void SetTile(int x, int y, FluidTileItem item)
        {
            if (ReferenceEquals(item,null)) {
                tilemap.SetTile(new Vector3Int(x,y,0),null);
                return;
            }
            Vector2Int vec = new Vector2Int(x,y);
            var tuple = GetPartitionFluidData(vec);
       
            if (tuple == null) return;
            var (fluidPartitionData, positionInPartition) = tuple.Value;
            int tileIndex = (int)(8 * fluidPartitionData.fill[positionInPartition.x, positionInPartition.y]);
            Tile tile = item.getTile(tileIndex);
            tilemap.SetTile(new Vector3Int(x,y,0),tile);
        }
        
        protected override void WriteTile(IChunkPartition partition, Vector2Int positionInPartition, FluidTileItem item)
        {
            PartitionFluidData partitionFluidData = partition.GetFluidData();
            partitionFluidData.ids[positionInPartition.x,positionInPartition.y] = item.id;
            partitionFluidData.fill[positionInPartition.x,positionInPartition.y] = MAX_FILL;
            Vector2Int cellPosition = partition.GetRealPosition() * Global.CHUNK_PARTITION_SIZE + positionInPartition;
            FluidCellData? nullableDual = GetDualData(cellPosition);
            if (nullableDual != null)
            {
                var dual = nullableDual.Value;
                dual.SetId(item.id);
                dual.SetFill(MAX_FILL);
            }
            AddFluidUpdate(cellPosition,item.id);
        }
        public void FixedUpdate()
        {
            ticks++;
            if (!tickFluidUpdates.Remove(ticks, out var positions)) return;
            return;
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (var position in positions)
            {
                var nullableOrgCellData = GetDualData(position);
                if (nullableOrgCellData == null) continue;
                var orgCellData = nullableOrgCellData.Value;
                
                string id = orgCellData.GetId();
                FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(id);
                if (ReferenceEquals(fluidTileItem,null)) continue;
                bool update = VerticalFluidUpdate(orgCellData, fluidTileItem);
                if (!update) continue;
                Vector2Int gravAdj = position + (fluidTileItem.fluidOptions.InvertedGravity ? Vector2Int.up : Vector2Int.down);
                refreshedPositions.Add(position);
                refreshedPositions.Add(gravAdj);
            }
            
            foreach (var position in refreshedPositions)
            {
                RefreshPosition(position,itemRegistry);
            }
            
            foreach (var position in positions)
            {
                if (refreshedPositions.Contains(position)) continue;
                var nullableOrgCellData = GetDualData(position);
                if (nullableOrgCellData == null) continue;
                var orgCellData = nullableOrgCellData.Value;
                
                string id = orgCellData.GetId();
                FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(id);
                if (ReferenceEquals(fluidTileItem,null)) continue;
                HorizontalFluidUpdate(orgCellData);
            }
            
            
            // Set dual array values to new values
            foreach (var position in positions)
            {
                foreach (Vector2Int adjPosition in toRefresh)
                {
                    if (refreshedPositions.Add(position+adjPosition)) RefreshPosition(position+adjPosition,itemRegistry);
                }
            }
            refreshedPositions.Clear();
        }


        private bool CanMergeHorizontalFluid(string id, float fill, FluidCellData? adjCell)
        {
            if (adjCell == null) return false;
            string adjBaseId = adjCell?.GetBaseId();
            if (adjBaseId != null) return false;
            
            string adjId = adjCell?.GetId();
            if (adjId != null && adjId != id) return false;
            
            float adjFill = adjCell?.GetFill() ?? 0;
            return fill > adjFill + EPSILON;
        }

        private void HorizontalMerge(FluidCellData fluidCellData, FluidCellData adjCellData, float divisor)
        {
            float fill = fluidCellData.GetFill();
            float adjFill = adjCellData.GetFill();
            float dif = (fill-adjFill) / divisor;
            adjCellData.SetPartitionId(fluidCellData.GetId());
            if (dif < EPSILON)
            {
                adjCellData.AddToPartitionFill(dif);
                return;
            }
            fluidCellData.AddToPartitionFill(-dif);
            adjCellData.AddToPartitionFill(dif);
        }
        /// <summary>
        /// Splits fluids horizontally
        /// Tiles with a higher fill than their neighbor give some to their neighbors.
        /// </summary>
        /// <param name="orgCellData"></param>
        private void HorizontalFluidUpdate(FluidCellData orgCellData)
        {
            Vector2Int position = orgCellData.CellPosition;
            FluidCellData? nullableLeft = GetDualData(position + Vector2Int.left);
            FluidCellData? nullableRight = GetDualData(position + Vector2Int.right);
            if (nullableLeft == null && nullableRight == null) return;

            
            string id = orgCellData.GetId();
            float orgFill = orgCellData.GetFill();
            
            bool canMergeLeft = CanMergeHorizontalFluid(id, orgFill, nullableLeft);
            bool canMergeRight = CanMergeHorizontalFluid(id, orgFill, nullableRight);

            if (!canMergeLeft && !canMergeRight) return;

            float divisor = canMergeLeft && canMergeRight ? 4f : 2f;

            if (canMergeLeft)
            {
                HorizontalMerge(orgCellData,nullableLeft.Value,divisor);
                
                
            }

            if (canMergeRight)
            {
                HorizontalMerge(orgCellData,nullableRight.Value,divisor);
               
                
            }
            
            AddFluidUpdate(position+Vector2Int.left,id);
            AddFluidUpdate(position+Vector2Int.right,id);
            AddFluidUpdate(position,id);
        }

        /// <summary>
        /// Fluid updates modify the actual partition data from the dual data. Update based on the real data and then sync dual to actual
        /// </summary>
        /// <param name="position"></param>
        /// <param name="itemRegistry"></param>
        private void RefreshPosition(Vector2Int position, ItemRegistry itemRegistry)
        {
            var chunkPosition= Global.getChunkFromCell(position);
            if (!dualChunkFluidData.TryGetValue(chunkPosition, out var dataArray)) return;
            Vector2Int partitionPosition = Global.getPartitionFromCell(position) - chunkPosition * Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = closedChunkSystem.getChunk(chunkPosition).GetPartition(partitionPosition);
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            PartitionFluidData partitionData = partition.GetFluidData();
            
            float fill = partitionData.fill[positionInPartition.x, positionInPartition.y];
            if (fill <= 0)
            {
                partitionData.ids[positionInPartition.x,positionInPartition.y] = null;
                partitionData.fill[positionInPartition.x,positionInPartition.y] = 0;
                SetTile(position.x,position.y,null);
            } else {
                partitionData.fill[positionInPartition.x,positionInPartition.y] = fill;
                string id = partitionData.ids[positionInPartition.x,positionInPartition.y];
                SetTile(position.x,position.y,itemRegistry.GetFluidTileItem(id));
            }
                
            PartitionFluidData dualData = dataArray[partitionPosition.x][partitionPosition.y];
            dualData.ids[positionInPartition.x, positionInPartition.y] = partitionData.ids[positionInPartition.x, positionInPartition.y];
            dualData.fill[positionInPartition.x, positionInPartition.y] = partitionData.fill[positionInPartition.x, positionInPartition.y];
        }

        private void SyncDualToPartition(Vector2Int position)
        {
            var chunkPosition= Global.getChunkFromCell(position);
            if (!dualChunkFluidData.TryGetValue(chunkPosition, out var dataArray)) return;
            Vector2Int partitionPosition = Global.getPartitionFromCell(position) - chunkPosition * Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = closedChunkSystem.getChunk(chunkPosition).GetPartition(partitionPosition);
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            PartitionFluidData partitionData = partition.GetFluidData();
            
            float fill = partitionData.fill[positionInPartition.x, positionInPartition.y];
            if (fill <= 0)
            {
                partitionData.ids[positionInPartition.x,positionInPartition.y] = null;
                partitionData.fill[positionInPartition.x,positionInPartition.y] = 0;
            } else {
                partitionData.fill[positionInPartition.x,positionInPartition.y] = fill;
            }
                
            PartitionFluidData dualData = dataArray[partitionPosition.x][partitionPosition.y];
            dualData.ids[positionInPartition.x, positionInPartition.y] = partitionData.ids[positionInPartition.x, positionInPartition.y];
            dualData.fill[positionInPartition.x, positionInPartition.y] = partitionData.fill[positionInPartition.x, positionInPartition.y];
        }

        public void tileUpdate(Vector2Int position)
        {
            AddFluidUpdate(position+Vector2Int.up);
            AddFluidUpdate(position+Vector2Int.left);
            AddFluidUpdate(position+Vector2Int.down);
            AddFluidUpdate(position+Vector2Int.right);
        }

        private void AddFluidUpdate(Vector2Int position)
        {
            var chunkPosition= Global.getChunkFromCell(position);
            if (!dualChunkFluidData.TryGetValue(chunkPosition, out var dataArray)) return;
            Vector2Int partitionPosition = Global.getPartitionFromCell(position) - chunkPosition * Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = closedChunkSystem.getChunk(chunkPosition).GetPartition(partitionPosition);
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            PartitionFluidData fluidPartitionData = partition.GetFluidData();
            AddFluidUpdate(position,fluidPartitionData.ids[positionInPartition.x,positionInPartition.y]);
        }
        
        private (PartitionFluidData,Vector2Int)? GetPartitionFluidData(Vector2Int position)
        {
            var (chunkPosition, partitionPosition, positionInPartition) = Global.GetChunkPartitionAndPositionInPartitionPositions(position);
            ILoadedChunk chunk = closedChunkSystem.getChunk(chunkPosition);
            IChunkPartition partition = chunk?.GetPartition(partitionPosition);
            if (partition == null) return null;
            return (partition.GetFluidData(),positionInPartition);
        }

        private FluidCellData? GetDualData(Vector2Int position)
        {
            var chunkPosition= Global.getChunkFromCell(position);
            if (!dualChunkFluidData.TryGetValue(chunkPosition, out var dataArray)) return null;
            Vector2Int partitionPosition = Global.getPartitionFromCell(position) - chunkPosition * Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = closedChunkSystem.getChunk(chunkPosition).GetPartition(partitionPosition);
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            return new FluidCellData(dataArray[partitionPosition.x][partitionPosition.y], partition, positionInPartition, position);
        }
        
        private FluidCellData? GetPartitionCellData(Vector2Int position)
        {
            var chunkPosition= Global.getChunkFromCell(position);
            if (!dualChunkFluidData.TryGetValue(chunkPosition, out var dataArray)) return null;
            Vector2Int partitionPosition = Global.getPartitionFromCell(position) - chunkPosition * Global.PARTITIONS_PER_CHUNK;
            IChunkPartition partition = closedChunkSystem.getChunk(chunkPosition).GetPartition(partitionPosition);
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            return new FluidCellData(partition.GetFluidData(), partition, positionInPartition, position);
        }

        private bool VerticalFluidUpdate(FluidCellData orgCellData, FluidTileItem fluidTileItem)
        {
            Vector2Int position = orgCellData.CellPosition;
            Vector2Int adjGravityPosition = fluidTileItem.fluidOptions.InvertedGravity ? position + Vector2Int.up : position + Vector2Int.down;
            var nullableGravCellData = GetDualData(adjGravityPosition);
            if (nullableGravCellData == null) return false;
            var gravCellData = nullableGravCellData.Value;
            if (TryFallUpdate(orgCellData, gravCellData)) return true;
            return TryMergeBelowUpdate(orgCellData, gravCellData);
        }

        private bool TryFallUpdate(FluidCellData orgCellData, FluidCellData gravCellData)
        {
            string baseId = gravCellData.GetBaseId();
            bool waterLogged; // TODO add water logging
            bool solidGravityAdj;
            CheckBaseTileProperties(baseId,out waterLogged,out solidGravityAdj);
            if (solidGravityAdj || gravCellData.GetId() != null) return false;
            string id = orgCellData.GetId();
            gravCellData.SetPartitionId(id);
            gravCellData.SetPartitionFill(orgCellData.GetFill());
            orgCellData.SetPartitionFill(0);
            orgCellData.SetPartitionId(null);
            
            Vector2Int above = orgCellData.CellPosition - gravCellData.CellPosition+orgCellData.CellPosition;
            AddFluidUpdate(above,id);
            AddFluidUpdate(gravCellData.CellPosition,id);
            
            return true;
        }
        
        

        private bool TryMergeBelowUpdate(FluidCellData orgCellData, FluidCellData adjCellData)
        {
            if (adjCellData.IsFull()) return false;
            string gravId = adjCellData.GetId();
            if (gravId == null || adjCellData.IsFull() || adjCellData.GetBaseId() != null || !gravId.Equals(orgCellData.GetId())) return false;
            float gravFill = adjCellData.GetFill();
            float orgFill = orgCellData.GetFill();
            if (gravFill > MAX_FILL)
            {
                gravFill = MAX_FILL;
                orgFill -= MAX_FILL;
            }
            else
            {
                gravFill = orgFill;
                orgFill = 0;
            }
            orgCellData.SetPartitionFill(orgFill);
            adjCellData.SetPartitionFill(gravFill);
            AddFluidUpdate(orgCellData.CellPosition,gravId);
            AddFluidUpdate(adjCellData.CellPosition,gravId);
            return true;
        }
        
        private static void CheckBaseTileProperties(string baseId, out bool waterLogged, out bool solid) {
            solid = false;
            waterLogged = false;
            
            if (baseId == null) return;
            TileItem tileItem = ItemRegistry.GetInstance().GetTileItem(baseId);
            if (ReferenceEquals(tileItem,null)) return;
            TileType tileType = tileItem.tileType;
            solid = tileType.isSolid();
            if (!solid || tileItem.tile is not Tile tile) {
                return;
            }
            waterLogged = tile.colliderType == Tile.ColliderType.Sprite;
        }
        private void AddFluidUpdate(Vector2Int position, string fluidItemId)
        {
            if (fluidItemId == null) return;
            
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            FluidTileItem fluidTileItem = itemRegistry.GetFluidTileItem(fluidItemId);
            if (ReferenceEquals(fluidTileItem,null)) return;
            uint viscosity = (uint)fluidTileItem.fluidOptions.Viscosity;
            if (viscosity == 0) return;
            
            uint updateTick = ticks + (uint)fluidTileItem.fluidOptions.Viscosity;
            if (!tickFluidUpdates.ContainsKey(updateTick))
            {
                tickFluidUpdates.Add(updateTick, new HashSet<Vector2Int>());
            }
            tickFluidUpdates[updateTick].Add(position);
        }
        private struct FluidCellData
        {
            public Vector2Int CellPosition;
            public IChunkPartition ChunkPartition;
            public PartitionFluidData PartitionFluidData;
            public Vector2Int PositionInPartition;

           
            public FluidCellData(PartitionFluidData partitionFluidData, IChunkPartition partition, Vector2Int positionInPartition, Vector2Int cellPosition)
            {
                // PartitionFluidData need not belong to the same partition
                PartitionFluidData = partitionFluidData;
                PositionInPartition = positionInPartition;
                ChunkPartition = partition;
                CellPosition = cellPosition;
            }

            public void SetId(string id)
            {
                PartitionFluidData.ids[PositionInPartition.x, PositionInPartition.y] = id;
            }
            
            public void SetPartitionId(string id)
            {
                ChunkPartition.GetFluidData().ids[PositionInPartition.x, PositionInPartition.y] = id;
            }
            
            public void SetPartitionFill(float fill)
            {
                ChunkPartition.GetFluidData().fill[PositionInPartition.x, PositionInPartition.y] = fill;
            }

            public void AddToPartitionFill(float amount)
            {
                ChunkPartition.GetFluidData().fill[PositionInPartition.x, PositionInPartition.y] += amount;
            }

            public string GetId()
            {
                return PartitionFluidData.ids[PositionInPartition.x, PositionInPartition.y];
            }
            public void SetFill(float fill)
            {
                PartitionFluidData.fill[PositionInPartition.x, PositionInPartition.y] = fill;
            }

            public float GetFill()
            {
                return PartitionFluidData.fill[PositionInPartition.x, PositionInPartition.y];
            }

            public string GetBaseId()
            {
                return ChunkPartition.GetData().baseData.ids[PositionInPartition.x, PositionInPartition.y];
            }

            public bool IsFull()
            {
                float fill = GetFill();
                return fill >= 1f;
            }
            
        }
    }
}

