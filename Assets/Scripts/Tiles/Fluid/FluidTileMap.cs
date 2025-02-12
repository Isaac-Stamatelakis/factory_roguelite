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

        protected override void WriteTile(IChunkPartition partition, Vector2Int position, FluidTileItem item)
        {
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            PartitionFluidData partitionFluidData = partition.GetFluidData();
            partitionFluidData.ids[positionInPartition.x,positionInPartition.y] = item.id;
            partitionFluidData.fill[positionInPartition.x,positionInPartition.y] = MAX_FILL;
            AddFluidUpdate(position,item.id);
        }
        public void FixedUpdate()
        {
            ticks++;
            if (!tickFluidUpdates.Remove(ticks, out var positions)) return;
            
            
            foreach (var position in positions)
            {
                var nullableOrgCellData = GetDualData(position);
                if (nullableOrgCellData == null) continue;
                var orgCellData = nullableOrgCellData.Value;
                
                string id = orgCellData.GetId();
                FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(id);
                if (ReferenceEquals(fluidTileItem,null)) continue;
                VerticalFluidUpdate(orgCellData, fluidTileItem);
            }
            
            foreach (var position in positions)
            {
                SyncDualToPartition(position);
                SyncDualToPartition(position+Vector2Int.down);
                SyncDualToPartition(position+Vector2Int.up);
            }
            
            foreach (var position in positions)
            {
                var nullableOrgCellData = GetDualData(position);
                if (nullableOrgCellData == null) continue;
                var orgCellData = nullableOrgCellData.Value;
                
                string id = orgCellData.GetId();
                FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(id);
                if (ReferenceEquals(fluidTileItem,null)) continue;
                HorizontalFluidUpdate(orgCellData);
            }
            
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            HashSet<Vector2Int> refreshedPositions = new HashSet<Vector2Int>();
            List<Vector2Int> toRefresh = new List<Vector2Int>
            {
                Vector2Int.zero,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up
            };
            
            // Set dual array values to new values
            foreach (var position in positions)
            {
                foreach (Vector2Int adjPosition in toRefresh)
                {
                    if (refreshedPositions.Add(position+adjPosition)) RefreshPosition(position+adjPosition,itemRegistry);
                }
            }
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
                //fluidCellData.SetPartitionFill(fill);
                //adjCellData.SetPartitionFill(fill);
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

            float divisor = canMergeLeft && canMergeRight ? 2f : 2f;

            if (canMergeLeft)
            {
                AddFluidUpdate(position+Vector2Int.left,id);
                HorizontalMerge(orgCellData,nullableLeft.Value,divisor);
            }

            if (canMergeRight)
            {
                AddFluidUpdate(position+Vector2Int.right,id);
                HorizontalMerge(orgCellData,nullableRight.Value,divisor);
            }
            
            
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

        private void VerticalFluidUpdate(FluidCellData orgCellData, FluidTileItem fluidTileItem)
        {
            Vector2Int position = orgCellData.CellPosition;
            Vector2Int adjGravityPosition = fluidTileItem.fluidOptions.InvertedGravity ? position + Vector2Int.up : position + Vector2Int.down;
            var nullableGravCellData = GetDualData(adjGravityPosition);
            if (nullableGravCellData == null) return;
            var gravCellData = nullableGravCellData.Value;
            if (TryFallUpdate(orgCellData, gravCellData)) return;
            TryMergeBelowUpdate(orgCellData, gravCellData);

        }

        private bool TryFallUpdate(FluidCellData orgCellData, FluidCellData gravCellData)
        {
            string baseId = gravCellData.GetBaseId();
            bool waterLogged; // TODO add water logging
            bool solidGravityAdj;
            CheckBaseTileProperties(baseId,out waterLogged,out solidGravityAdj);
            if (solidGravityAdj || gravCellData.GetId() != null) return false;
            gravCellData.SetPartitionId(orgCellData.GetId());
            gravCellData.SetPartitionFill(orgCellData.GetFill());
            orgCellData.SetPartitionFill(0);
            orgCellData.SetPartitionId(null);

            tileUpdate(orgCellData.CellPosition);
            TryMergeBelowUpdate(orgCellData, gravCellData);
            
            return true;
        }
        
        

        private void TryMergeBelowUpdate(FluidCellData orgCellData, FluidCellData adjCellData)
        {
            if (adjCellData.IsFull()) return;
            string gravId = adjCellData.GetId();
            if (gravId == null || adjCellData.GetBaseId() != null || !gravId.Equals(orgCellData.GetId())) return;
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
            
        }
        private void DoFluidUpdate(FluidCellData orgCellData, Vector2Int position)
        {
            
            
            /*
            CellFluidData leftFluidData = getFluidData(position + Vector2Int.left,partitionPosition,centerPartitionData);
            CellFluidData rightFluidData = getFluidData(position + Vector2Int.right,partitionPosition,centerPartitionData);

            bool canMergeLeft = (leftFluidData.baseId == null && leftFluidData.fluidPartitionData != null && (leftFluidData.id == null || leftFluidData.id == cellFluidData.id));
            bool canMergeRight = (rightFluidData.baseId == null && rightFluidData.fluidPartitionData != null && (rightFluidData.id == null || rightFluidData.id == cellFluidData.id));

            if (!canMergeLeft && !canMergeRight) {
                return;
            }
            float epilson = 0.0001f;
            bool mergeLeft = canMergeLeft && cellFluidData.fill > leftFluidData.fill;
            bool mergeRight = canMergeRight && cellFluidData.fill > rightFluidData.fill;
            float divisor = 2f;
            if (mergeLeft && mergeRight) {
                divisor = 4f;
            }
            if (mergeLeft) {
                float left = (cellFluidData.fill-leftFluidData.fill)/divisor;
                if (left < epilson) {
                    cellFluidData.fill=leftFluidData.fill;
                } else {
                    cellFluidData.fill -= left;
                    leftFluidData.fill += left;
                }
                leftFluidData.id = cellFluidData.id;
                setUpdatedFluidTile(leftFluidData);
            }

            if (mergeRight) {
                float right = (cellFluidData.fill-rightFluidData.fill)/divisor;
                if (right < epilson) {
                    cellFluidData.fill = leftFluidData.fill;
                } else {
                    cellFluidData.fill -= right;
                    rightFluidData.fill += right;
                }
                rightFluidData.id = cellFluidData.id;
                setUpdatedFluidTile(rightFluidData);
            }
            tileUpdate(cellFluidData.position);
            setUpdatedFluidTile(cellFluidData);
            */

            /*
            if (averageDifferent(cellFluidData.fill,avg,epilson)) {
                cellFluidData.fill = avg;
                setUpdatedFluidTile(cellFluidData);
            }

            if (canMergeLeft && averageDifferent(leftFluidData.fill,avg,epilson)) {
                leftFluidData.id = cellFluidData.id;
                leftFluidData.fill = avg;
                setUpdatedFluidTile(leftFluidData);

            }
            if (canMergeRight && averageDifferent(rightFluidData.fill,avg,epilson)) {
                rightFluidData.id = cellFluidData.id;
                rightFluidData.fill = avg;
                setUpdatedFluidTile(rightFluidData);
            }
            */


            /*
            bool canMergeLeft = (leftFluidData.baseId == null && leftFluidData.fluidPartitionData != null && ((leftFluidData.id == null || leftFluidData.id == centerId) || leftFluidData.fill < centerFill));
            bool canMergeRight = (rightFluidData.baseId == null && rightFluidData.fluidPartitionData != null && ((rightFluidData.id == null || rightFluidData.id == centerId) || rightFluidData.fill < centerFill));
            if (!canMergeLeft && !canMergeRight) {
                return;
            }

            if (canMergeLeft && canMergeRight) {
                float leftChange = (centerFill-leftFluidData.fill)/4f;
                float rightChange = (centerFill-rightFluidData.fill)/4f;
                leftFluidData.fill += Mathf.RoundToInt(leftChange);
                rightFluidData.fill += Mathf.RoundToInt(rightChange);
                centerFill -= Mathf.CeilToInt(leftChange+rightChange);
            }
            if (canMergeLeft) {
                float leftChange = (centerFill-leftFluidData.fill)/2f;
                leftFluidData.fill += Mathf.RoundToInt(leftChange);
                centerFill -= Mathf.CeilToInt(leftChange);
            }
            if (canMergeRight) {
                float rightChange = (centerFill-rightFluidData.fill)/2f;
                rightFluidData.fill += Mathf.RoundToInt(rightChange);
                centerFill -= Mathf.CeilToInt(rightChange);
            }
            */
            /*
            if (canMergeLeft) {
                if (leftFluidData.id == null) {
                    leftFluidData.id = centerId;
                }
                setUpdatedFluidTile(leftFluidData);
                addFluidUpdate(leftFluidData.position);
            }
            if (canMergeRight) {
                if (rightFluidData.id == null) {
                    rightFluidData.id = centerId;
                }
                setUpdatedFluidTile(rightFluidData);
                addFluidUpdate(rightFluidData.position);
            }
            setUpdatedFluidTile(
                partitionData: centerPartitionData,
                posInPartition: centerPositionInPartition,
                position: position,
                fill: centerFill,
                id: centerId,
                fluidTileItem: fluidTileItem
            );
            */
            //addFluidUpdate(position);

        }

        /*
        private bool averageDifferent(float fill, float avg, float epilson) {
            return Mathf.Abs(fill-avg) >= epilson;  
        }

        private void addUpdateFill(Vector2Int position, float fill) {
            if (updateFills.ContainsKey(position)) {
                float current = updateFills[position];
                if (fill < current) {
                    updateFills[position] = fill;
                }
            } else {
                updateFills[position] = fill;
            }
        }
        


        private void setUpdatedFluidTile(CellFluidData cellFluidData) {
            ActiveFluidPartitionData partitionData = cellFluidData.fluidPartitionData;
            Vector2Int posInPartition = cellFluidData.positionInPartition;
            Vector2Int position = cellFluidData.position;
            float fill = cellFluidData.fill;
            string id = cellFluidData.id;
            FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(id);
            int x = posInPartition.x;
            int y = posInPartition.y;
            if (partitionData == null) {
                return;
            }
            if (fill <= 0.01f) {
                partitionData.ids[x,y] = null;
                partitionData.fill[x,y] = 0;
                SetTile(position.x,position.y,null);
            } else {
                partitionData.ids[x,y] = id;
                partitionData.fill[x,y] = fill;
                SetTile(position.x,position.y,fluidTileItem);
            }
        }
        
        
        */
        
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
        
        /*
        private struct CellFluidData {
            public CellFluidData(Vector2Int position, Vector2Int partitionPosition, Vector2Int positionInPartition, ActiveFluidPartitionData fluidPartitionData) {
                this.position = position;
                this.partitionPosition = partitionPosition;
                this.positionInPartition = positionInPartition;
                this.fluidPartitionData = fluidPartitionData;
                if (fluidPartitionData != null) {
                    id = fluidPartitionData.ids[positionInPartition.x,positionInPartition.y];
                    fill = fluidPartitionData.fill[positionInPartition.x,positionInPartition.y];
                    if (fill > 1f) {
                        fluidPartitionData.fill[positionInPartition.x,positionInPartition.y] = 1f;
                        fill = 1f;
                    } 
                    baseId = fluidPartitionData.baseIds[positionInPartition.x,positionInPartition.y];
                } else {
                    id = null;
                    fill = 0;
                    baseId = null;
                }
                full = fill >= 1;
            }

            public void setId(string id) {
                fluidPartitionData.ids[positionInPartition.x,positionInPartition.y] = id;
                this.id = id;
            }
            public Vector2Int position;
            public Vector2Int partitionPosition;
            public Vector2Int positionInPartition;
            public ActiveFluidPartitionData fluidPartitionData;
            public string id;
            public float fill;
            public bool full;
            public string baseId;
        }
        */
    }
}

