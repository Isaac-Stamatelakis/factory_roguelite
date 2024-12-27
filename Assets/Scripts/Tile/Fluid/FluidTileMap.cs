using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps;
using Chunks.Partitions;
using UnityEngine.Tilemaps;
using Items;
using System.Linq;

namespace Fluids {
    public class FluidTileMap : AbstractTileMap<FluidTileItem>, ITileMapListener
    {
        private Dictionary<Vector2Int,ActiveFluidPartitionData> partitionFluidData = new Dictionary<Vector2Int, ActiveFluidPartitionData>();
        private Dictionary<Vector2Int, int> fluidUpdates = new Dictionary<Vector2Int, int>();
        private Dictionary<Vector2Int, float> updateFills = new Dictionary<Vector2Int, float>();
        public override void Start()
        {
            base.Start();
        }
        public override void hitTile(Vector2 position)
        {
            
        }

        protected override Vector2Int GetHitTilePosition(Vector2 position)
        {
            return Global.getCellPositionFromWorld(position);
        }

        protected override void SetTile(int x, int y, FluidTileItem item)
        {
            if (item == null) {
                tilemap.SetTile(new Vector3Int(x,y,0),null);
                return;
            }
            Vector2Int vec = new Vector2Int(x,y);
            Vector2Int partitionPosition = Global.getPartitionFromCell(vec);
            Vector2Int positionInPartition = Global.getPositionInPartition(vec);
            ActiveFluidPartitionData fluidPartitionData = partitionFluidData[partitionPosition];
            int tileIndex = Mathf.RoundToInt (8 * fluidPartitionData.fill[positionInPartition.x,positionInPartition.y]);
            Tile tile = item.getTile(tileIndex);
            tilemap.SetTile(new Vector3Int(x,y,0),tile);
        }

        protected override void WriteTile(IChunkPartition partition, Vector2Int position, FluidTileItem item)
        {
            Vector2Int realPosition = partition.getRealPosition()*Global.ChunkPartitionSize + position;
            ActiveFluidPartitionData data = partitionFluidData[partition.getRealPosition()];
            data.ids[position.x,position.y] = item.id;
            data.fill[position.x,position.y] = 8;
            addFluidUpdate(new Vector2Int(realPosition.x,realPosition.y));
        }
        public override IEnumerator removePartition(Vector2Int partitionPosition)
        {
            partitionFluidData.Remove(partitionPosition);
            return base.removePartition(partitionPosition);
        }


        public override void addPartition(IChunkPartition partition)
        {
            base.addPartition(partition);
            partitionFluidData[partition.getRealPosition()] = new ActiveFluidPartitionData(partition.getFluidData()); 
        }

        public void FixedUpdate() {
            HashSet<Vector2Int> positionsHitThisUpdate = new HashSet<Vector2Int>();
            foreach (KeyValuePair<Vector2Int,int> kvp in fluidUpdates.ToList()) {
                Vector2Int position = kvp.Key;
                int updateTime = kvp.Value;
                fluidUpdates[kvp.Key] = updateTime-1;
                if (updateTime >= 0) {
                    continue;
                }
                fluidUpdates.Remove(kvp.Key);
                positionsHitThisUpdate.Add(position);
                doFluidUpdate(position);
            }
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (var kvp in updateFills) {
                Vector2Int position = kvp.Key;
                float fill = kvp.Value;
                Vector2Int partitionPosition = Global.getPartitionFromCell(position);
                if (!partitionFluidData.ContainsKey(partitionPosition)) {
                    continue;
                }
                ActiveFluidPartitionData partitionData = partitionFluidData[partitionPosition];
                if (partitionData == null) {
                    return;
                }
                Vector2Int positionInPartition = Global.getPositionInPartition(position);
                int x = positionInPartition.x;
                int y = positionInPartition.y;
                
                if (fill <= 0) {
                    partitionData.ids[x,y] = null;
                    partitionData.fill[x,y] = 0;
                    SetTile(position.x,position.y,null);
                } else {
                    partitionData.fill[x,y] = fill;
                    string id = partitionData.ids[x,y];
                    SetTile(position.x,position.y,itemRegistry.GetFluidTileItem(id));
                }
                //addFluidUpdate(position);
            }
            if (updateFills.Count > 0) {
                updateFills = new Dictionary<Vector2Int, float>();
            }
            
        }
        public void tileUpdate(Vector2Int position)
        {
            addFluidUpdate(position+Vector2Int.up);
            addFluidUpdate(position+Vector2Int.left);
            addFluidUpdate(position+Vector2Int.down);
            addFluidUpdate(position+Vector2Int.right);
        }

        private void doFluidUpdate(Vector2Int position) {
            Vector2Int partitionPosition = Global.getPartitionFromCell(position);
            if (!partitionFluidData.ContainsKey(partitionPosition)) {
                return;
            }
            Vector2Int centerPositionInPartition = Global.getPositionInPartition(position);
            ActiveFluidPartitionData centerPartitionData = partitionFluidData[partitionPosition];
            CellFluidData cellFluidData = getFluidData(position,centerPositionInPartition,centerPartitionData);
            FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(cellFluidData.id);
            if (fluidTileItem == null) {
                return;
            }

            
            Vector2Int adjGravityPosition = fluidTileItem.fluidOptions.InvertedGravity ? position + Vector2Int.up : position + Vector2Int.down;
            CellFluidData adjGravityData = getFluidData(adjGravityPosition,partitionPosition,centerPartitionData);
            
            // Fall / Rise
            string baseId = adjGravityData.baseId;
            bool waterLogged;
            bool solidGravityAdj;
            checkBaseTileProperties(baseId,out waterLogged,out solidGravityAdj);
            if (!solidGravityAdj && adjGravityData.id == null && adjGravityData.fluidPartitionData != null) { 
                adjGravityData.id = cellFluidData.id;
                adjGravityData.fill = cellFluidData.fill;
                cellFluidData.fill = 0;
                setUpdatedFluidTile(cellFluidData);
                setUpdatedFluidTile(adjGravityData);
                tileUpdate(position);
                tileUpdate(adjGravityData.position);
                return;
            }

            // Merge below
            if (!adjGravityData.full && adjGravityData.id != null && adjGravityData.baseId == null && adjGravityData.id.Equals(cellFluidData.id)) {
                adjGravityData.fill += cellFluidData.fill;
                bool full = adjGravityData.fill >= 1f;
                adjGravityData.full = full;
                if (full) {
                    cellFluidData.fill = adjGravityData.fill - 1f;
                    adjGravityData.fill = 1f;
                } else {
                    cellFluidData.fill = 0;
                }
                setUpdatedFluidTile(cellFluidData);
                setUpdatedFluidTile(adjGravityData);
                addFluidUpdate(cellFluidData.position);
                addFluidUpdate(adjGravityData.position);
            }

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
        
        private CellFluidData getFluidData(Vector2Int position, Vector2Int centerPartitionPosition, ActiveFluidPartitionData centerFluidPartitionData) {
            Vector2Int partitionPosition = Global.getPartitionFromCell(position);
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            ActiveFluidPartitionData fluidPartitionData = null;
            if (centerFluidPartitionData.Equals(partitionPosition)) {
                fluidPartitionData = centerFluidPartitionData;
            } else if (partitionFluidData.ContainsKey(partitionPosition)) {
                fluidPartitionData = partitionFluidData[partitionPosition];
            }
            return new CellFluidData(
                position,
                partitionPosition,
                positionInPartition,
                fluidPartitionData
            );
        }
        private void checkBaseTileProperties(string baseId, out bool waterLogged, out bool solid) {
            solid = false;
            waterLogged = false;
            // TODO: Add support for tile blocks of large sizes (not sure if this is worth could just have a hard restriction on size)
            if (baseId == null) {
                return;
            }
            TileItem tileItem = ItemRegistry.GetInstance().GetTileItem(baseId);
            if (tileItem == null) {
                return;
            }
            TileType tileType = tileItem.tileType;
            solid = tileType.isSolid();
            if (!solid || tileItem.tile is not Tile tile) {
                return;
            }
            waterLogged = tile.colliderType == Tile.ColliderType.Sprite;
        }

        private void addFluidUpdate(Vector2Int position) {
            if (fluidUpdates.ContainsKey(position)) {
                return;
            }
            FluidTileItem fluidTileItem = getFluidTileItem(position);
            if (fluidTileItem == null) {
                return;
            }
            fluidUpdates[position] = fluidTileItem.fluidOptions.Viscosity;
        }

        private FluidTileItem getFluidTileItem(Vector2Int position) {
            Vector2Int partitionPosition = Global.getPartitionFromCell(position);
            if (!partitionFluidData.ContainsKey(partitionPosition)) {
                return null;
            }
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            string id = partitionFluidData[partitionPosition].ids[positionInPartition.x,positionInPartition.y];
            if (id == null) {
                return null;
            }
            return ItemRegistry.GetInstance().GetFluidTileItem(id);
        }

        private FluidData? getFluidData(Vector2Int position) {
            Vector2Int partitionPosition = Global.getPartitionFromCell(position);
            if (!partitionFluidData.ContainsKey(partitionPosition)) {
                return null;
            }
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            string id = partitionFluidData[partitionPosition].ids[positionInPartition.x,positionInPartition.y];
            if (id == null) {
                return null;
            }
            float fill = partitionFluidData[partitionPosition].fill[positionInPartition.x,positionInPartition.y];
            return new FluidData(fill,id);
        }
        private class ActiveFluidPartitionData {
            public string[,] ids;
            public string[,] baseIds;
            public float[,] fill;
            public ActiveFluidPartitionData(PartitionFluidData partitionFluidData) {
                this.ids = partitionFluidData.ids;
                this.baseIds = partitionFluidData.baseIds;
                this.fill = partitionFluidData.fill;
            }
        }

        private struct FluidData {
            public float fill;
            public string id;

            public FluidData(float fill, string id)
            {
                this.fill = fill;
                this.id = id;
            }
        }
        
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
    }
}

