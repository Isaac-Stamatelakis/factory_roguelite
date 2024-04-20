using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule;
using ChunkModule.PartitionModule;
using UnityEngine.Tilemaps;
using ItemModule;
using System.Linq;

namespace Fluids {
    public class FluidTileMap : AbstractTileMap<FluidTileItem>, ITileMapListener
    {
        private Dictionary<Vector2Int,ActiveFluidPartitionData> partitionFluidData = new Dictionary<Vector2Int, ActiveFluidPartitionData>();
        private Dictionary<Vector2Int, int> fluidUpdates = new Dictionary<Vector2Int, int>();
        private Dictionary<Vector2Int, int> updateFills = new Dictionary<Vector2Int, int>();
        public override void Start()
        {
            base.Start();
        }
        public override void hitTile(Vector2 position)
        {
            
        }

        protected override Vector2Int getHitTilePosition(Vector2 position)
        {
            return Global.getCellPositionFromWorld(position);
        }

        protected override void setTile(int x, int y, FluidTileItem item)
        {
            if (item == null) {
                tilemap.SetTile(new Vector3Int(x,y,0),null);
                return;
            }
            Vector2Int vec = new Vector2Int(x,y);
            Vector2Int partitionPosition = Global.getPartitionFromCell(vec);
            Vector2Int positionInPartition = Global.getPositionInPartition(vec);
            ActiveFluidPartitionData fluidPartitionData = partitionFluidData[partitionPosition];
            Tile tile = item.getTile(fluidPartitionData.fill[positionInPartition.x,positionInPartition.y]);
            tilemap.SetTile(new Vector3Int(x,y,0),tile);
        }

        protected override void writeTile(IChunkPartition partition, Vector2Int position, FluidTileItem item)
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
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            foreach (var kvp in updateFills) {
                Vector2Int position = kvp.Key;
                int fill = kvp.Value;
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
                    setTile(position.x,position.y,null);
                } else {
                    partitionData.fill[x,y] = fill;
                    string id = partitionData.ids[x,y];
                    setTile(position.x,position.y,itemRegistry.getFluidTileItem(id));
                }
                //addFluidUpdate(position);
            }
            if (updateFills.Count > 0) {
                updateFills = new Dictionary<Vector2Int, int>();
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
            string centerId = centerPartitionData.ids[centerPositionInPartition.x,centerPositionInPartition.y];
            if (centerId == null) {
                return;
            }
            FluidTileItem fluidTileItem = ItemRegistry.getInstance().getFluidTileItem(centerId);
            if (fluidTileItem == null) {
                return;
            }
            int centerFill = centerPartitionData.fill[centerPositionInPartition.x,centerPositionInPartition.y];

            Vector2Int adjGravityPosition = fluidTileItem.fluidOptions.InvertedGravity ? position + Vector2Int.up : position + Vector2Int.down;
            CellFluidData gravityAdjFluidData = getFluidData(adjGravityPosition,partitionPosition,centerPartitionData);
            
            bool changedBelow = mergeGravityFluid(
                position: position,
                partitionPosition: partitionPosition,
                centerPositionInPartition: centerPositionInPartition,
                centerFluidTile: fluidTileItem,
                invertedGravity: fluidTileItem.fluidOptions.InvertedGravity,
                centerPartitionData: centerPartitionData,
                adjGravityData: gravityAdjFluidData
            );
            if (changedBelow) {
                return;
            }
            
            string baseId = gravityAdjFluidData.baseId;
            bool waterLogged;
            bool solidGravityAdj;
            checkBaseTileProperties(baseId,out waterLogged,out solidGravityAdj);
            if (!solidGravityAdj && gravityAdjFluidData.id == null && gravityAdjFluidData.fluidPartitionData != null) { // Fall / Rise
                setUpdatedFluidTile(
                    partitionData: centerPartitionData,
                    posInPartition: centerPositionInPartition,
                    position,
                    0,
                    null,
                    null
                );
                setUpdatedFluidTile(
                    partitionData: gravityAdjFluidData.fluidPartitionData,
                    posInPartition: gravityAdjFluidData.positionInPartition,
                    position: gravityAdjFluidData.position,
                    centerFill,
                    centerId,
                    fluidTileItem
                );
                tileUpdate(position);
                return;
            }
            
            CellFluidData leftFluidData = getFluidData(position + Vector2Int.left,partitionPosition,centerPartitionData);
            CellFluidData rightFluidData = getFluidData(position + Vector2Int.right,partitionPosition,centerPartitionData);
            
            bool canMergeLeft = (leftFluidData.baseId == null && leftFluidData.fluidPartitionData != null);
            bool canMergeRight = (rightFluidData.baseId == null && rightFluidData.fluidPartitionData != null);
            if (!canMergeLeft && !canMergeRight) {
                return;
            }
            int sum = (centerFill + leftFluidData.fill + rightFluidData.fill);
            if (sum < 3) { // Prevents infinite duping
                return; 
            }
            int avg = 0;
            if (canMergeLeft && canMergeRight) {
                avg = Mathf.CeilToInt(sum/3f);
            } else {
                avg = Mathf.CeilToInt(sum/2f);
            }
            if (avg > 8) {
                avg = 8;
            }
            
            addUpdateFill(position,avg);
            tileUpdate(position);
            tileUpdate(position+Vector2Int.left);
            tileUpdate(position+Vector2Int.right);
            if (canMergeLeft) {
                addUpdateFill(leftFluidData.position,avg);
                leftFluidData.setId(centerId);
            }
            if (canMergeRight) {
                addUpdateFill(rightFluidData.position,avg);
                rightFluidData.setId(centerId);
            }
            
            
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

        private void addUpdateFill(Vector2Int position, int fill) {
            if (updateFills.ContainsKey(position)) {
                int current = updateFills[position];
                if (fill < current) {
                    updateFills[position] = fill;
                }
            } else {
                updateFills[position] = fill;
            }
        }


        private void setUpdatedFluidTile(
            ActiveFluidPartitionData partitionData, 
            Vector2Int posInPartition, 
            Vector2Int position, 
            int fill,
            string id,
            FluidTileItem fluidTileItem
        ) {
            int x = posInPartition.x;
            int y = posInPartition.y;
            if (partitionData == null) {
                return;
            }
            if (fill <= 0) {
                partitionData.ids[x,y] = null;
                partitionData.fill[x,y] = 0;
                setTile(position.x,position.y,null);
            } else {
                partitionData.ids[x,y] = id;
                partitionData.fill[x,y] = fill;
                setTile(position.x,position.y,fluidTileItem);
            }
        }
        private void setUpdatedFluidTile(CellFluidData cellFluidData) {
            
            setUpdatedFluidTile(
                cellFluidData.fluidPartitionData,
                cellFluidData.positionInPartition,
                cellFluidData.position,
                cellFluidData.fill,
                cellFluidData.id,
                ItemRegistry.getInstance().getFluidTileItem(cellFluidData.id)
            );
            
            
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
            TileItem tileItem = ItemRegistry.getInstance().getTileItem(baseId);
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

        private bool mergeGravityFluid(Vector2Int position, Vector2Int partitionPosition, Vector2Int centerPositionInPartition, FluidTileItem centerFluidTile, ActiveFluidPartitionData centerPartitionData, bool invertedGravity,  CellFluidData adjGravityData) {
            if (adjGravityData.fluidPartitionData == null || adjGravityData.id == null || !adjGravityData.id.Equals(centerFluidTile.id) || adjGravityData.full) {
                return false;
            }
            int gravFill = adjGravityData.fill;
            if (gravFill >= FluidTileHelper.MaxFill) {
                return false;
            }
            int centerFill = centerPartitionData.fill[centerPositionInPartition.x,centerPositionInPartition.y];
            gravFill += centerFill;
            centerFill = 0;
            /*
            if (gravFill > FluidTileHelper.MaxFill) {
                centerFill = gravFill - FluidTileHelper.MaxFill;
                gravFill = FluidTileHelper.MaxFill;
            } else {
                centerFill = 0;
            }
            */
            setUpdatedFluidTile(
                centerPartitionData,
                centerPositionInPartition,
                position,
                centerFill,
                centerFluidTile.id,
                centerFluidTile
            );
            setUpdatedFluidTile(
                adjGravityData.fluidPartitionData,
                adjGravityData.positionInPartition,
                adjGravityData.position,
                gravFill,
                adjGravityData.id,
                ItemRegistry.getInstance().getFluidTileItem(adjGravityData.id)
            );
            addFluidUpdate(position);
            addFluidUpdate(adjGravityData.position);
            return true;
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
            return ItemRegistry.getInstance().getFluidTileItem(id);
        }

        private (string, int)? getFluidData(Vector2Int position) {
            Vector2Int partitionPosition = Global.getPartitionFromCell(position);
            if (!partitionFluidData.ContainsKey(partitionPosition)) {
                return null;
            }
            Vector2Int positionInPartition = Global.getPositionInPartition(position);
            string id = partitionFluidData[partitionPosition].ids[positionInPartition.x,positionInPartition.y];
            if (id == null) {
                return null;
            }
            int fill = partitionFluidData[partitionPosition].fill[positionInPartition.x,positionInPartition.y];
            return (id,fill);
        }
        private class ActiveFluidPartitionData {
            public string[,] ids;
            public string[,] baseIds;
            public int[,] fill;
            public ActiveFluidPartitionData((string[,], string[,], int[,]) data) {
                this.ids = data.Item1;
                this.baseIds = data.Item2;
                this.fill = data.Item3;
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
                    baseId = fluidPartitionData.baseIds[positionInPartition.x,positionInPartition.y];
                } else {
                    id = null;
                    fill = 0;
                    baseId = null;
                }
                full = fill == FluidTileHelper.MaxFill;
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
            public int fill;
            public bool full;
            public string baseId;
        }
    }
}

