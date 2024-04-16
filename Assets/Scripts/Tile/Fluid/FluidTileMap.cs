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
            ActiveFluidPartitionData data = partitionFluidData[partition.getRealPosition()];
            data.ids[position.x,position.y] = item.id;
            data.fill[position.x,position.y] = 8;
        }
        public override IEnumerator removePartition(Vector2Int partitionPosition)
        {
            partitionFluidData.Remove(partitionPosition);
            return base.removePartition(partitionPosition);
        }


        public override void addPartition(IChunkPartition partition)
        {

            partitionFluidData[partition.getRealPosition()] = new ActiveFluidPartitionData(partition.getFluidData()); 
            base.addPartition(partition);
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
            FluidTileItem fluidTileItem = ItemRegistry.getInstance().getFluidTileItem(centerId);
            if (fluidTileItem == null) {
                return;
            }
            int centerFill = centerPartitionData.fill[centerPositionInPartition.x,centerPositionInPartition.y];
            ActiveFluidPartitionData adjacentGravityData;
            Vector2Int adjGravPosInPartition;
            Vector2Int adjacentGravityPosition;
            bool changedBelow = checkFluidBelow(
                position,
                partitionPosition,
                centerPositionInPartition,
                fluidTileItem,
                centerPartitionData,
                fluidTileItem.fluidOptions.InvertedGravity,
                checkedPartitionData: out adjacentGravityData,
                checkedPositionInPartition: out adjGravPosInPartition,
                checkPosition: out adjacentGravityPosition
                
            );
            if (changedBelow || adjacentGravityData == null) {
                return;
            }
            Debug.Log("A");

            string baseId = adjacentGravityData.baseIds[adjGravPosInPartition.x,adjGravPosInPartition.y];
            bool waterLogged;
            bool solid;
            checkBaseTileProperties(baseId,out waterLogged,out solid);
            bool adjacentGravityFluidNull = adjacentGravityData.ids[adjGravPosInPartition.x,adjGravPosInPartition.y] == null;
            if (!solid && adjacentGravityFluidNull) {
                centerPartitionData.ids[centerPositionInPartition.x,centerPositionInPartition.y] = null;
                centerPartitionData.fill[centerPositionInPartition.x,centerPositionInPartition.y] = 0;
                setTile(position.x,position.y,null);
                adjacentGravityData.ids[adjGravPosInPartition.x,adjGravPosInPartition.y] = centerId;
                adjacentGravityData.fill[adjGravPosInPartition.x,adjGravPosInPartition.y] = centerFill;
                setTile(adjacentGravityPosition.x,adjacentGravityPosition.y,fluidTileItem);
                tileUpdate(position);
                return;
            }
        
            Vector2Int leftPosition = position + Vector2Int.left;
            Vector2Int leftPartitionPosition;
            Vector2Int leftPositionInPartition;
            ActiveFluidPartitionData leftFluidPartitionData;
            getFluidData(position,partitionPosition,centerPartitionData,out leftPartitionPosition,out leftPositionInPartition,out leftFluidPartitionData);
            string leftId = leftFluidPartitionData.ids[leftPositionInPartition.x,leftPositionInPartition.y];
            bool fluidOnLeft = leftFluidPartitionData != null && leftId != null; 


            Vector2Int rightPosition = position + Vector2Int.right;
            Vector2Int rightPartitionPosition;
            Vector2Int rightPositionInPartition;
            ActiveFluidPartitionData rightFluidPartitionData;
            getFluidData(position,partitionPosition,centerPartitionData,out rightPartitionPosition,out rightPositionInPartition,out rightFluidPartitionData);
            string rightId = rightFluidPartitionData.ids[rightPositionInPartition.x,rightPositionInPartition.y];
            bool fluidOnRight = rightFluidPartitionData != null && rightId != null;
            
            if (!fluidOnLeft && !fluidOnRight) {
                return;
            }
            centerFill = centerFill/2;
            setUpdatedFluidTile(
                partitionData: centerPartitionData,
                posInPartition: centerPositionInPartition,
                position: position,
                fill: centerFill,
                id: centerId,
                fluidTileItem: fluidTileItem
            );
            int leftFill = 0;
            int rightFill = 0;
            if (fluidOnLeft && fluidOnRight) {
                leftFill = centerFill/4;
                rightFill = centerFill/4;
            }
            if (fluidOnLeft) {
                leftFill = centerFill/2;
            }
            if (fluidOnRight) {
                rightFill = centerFill/2;
            }
            if (fluidOnLeft) {
                setUpdatedFluidTile(
                    partitionData: leftFluidPartitionData,
                    posInPartition: leftPositionInPartition,
                    position: leftPosition,
                    fill: leftFill,
                    id: leftId,
                    fluidTileItem: ItemRegistry.getInstance().getFluidTileItem(leftId)
                );
            }
            if (fluidOnRight) {
                setUpdatedFluidTile(
                    partitionData: rightFluidPartitionData,
                    posInPartition: rightPositionInPartition,
                    position: rightPosition,
                    fill: rightFill,
                    id: rightId,
                    fluidTileItem: ItemRegistry.getInstance().getFluidTileItem(rightId)
                );
            }
            tileUpdate(position);

        }

        private void setUpdatedFluidTile(
            ActiveFluidPartitionData partitionData, 
            Vector2Int posInPartition, 
            Vector2Int position, 
            int fill,
            string id,
            FluidTileItem fluidTileItem
        ) {
            partitionData.ids[posInPartition.x,posInPartition.y] = id;
            partitionData.fill[posInPartition.x,posInPartition.y] = fill;
            setTile(position.x,position.y,fluidTileItem);
        }

        private void getFluidData(Vector2Int position, Vector2Int centerPartitionPosition, ActiveFluidPartitionData centerFluidPartitionData, out Vector2Int partitionPosition, out Vector2Int positionInPartition, out ActiveFluidPartitionData fluidPartitionData) {
            partitionPosition = Global.getPartitionFromCell(position);
            positionInPartition = Global.getPositionInPartition(position);
            if (centerFluidPartitionData.Equals(partitionPosition)) {
                fluidPartitionData = centerFluidPartitionData;
                return;
            }
            if (!partitionFluidData.ContainsKey(partitionPosition)) {
                fluidPartitionData = null;
                return;
            }
            fluidPartitionData = partitionFluidData[partitionPosition];
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

        private bool checkFluidBelow(Vector2Int position, Vector2Int partitionPosition, Vector2Int centerPositionInPartition, FluidTileItem centerFluidTile, ActiveFluidPartitionData centerPartitionData, bool invertedGravity, out ActiveFluidPartitionData checkedPartitionData, out Vector2Int checkedPositionInPartition, out Vector2Int checkPosition) {
            checkPosition = invertedGravity ? position + Vector2Int.up : position + Vector2Int.down;
            Vector2Int checkPartition = Global.getPartitionFromCell(checkPosition);
            Vector2Int checkPositionInPartition = Global.getPositionInPartition(checkPosition);
            ActiveFluidPartitionData checkFluidPartitionData = null;
            checkedPositionInPartition = checkPositionInPartition;
            if (checkPartition.Equals(partitionPosition)) {
                checkFluidPartitionData = centerPartitionData;
            } else if (partitionFluidData.ContainsKey(checkPartition)) {
                checkFluidPartitionData = partitionFluidData[checkPartition];
            }
            checkedPartitionData = checkFluidPartitionData;
            if (checkFluidPartitionData == null) {
                return false;
            }
            string downId = checkFluidPartitionData.ids[checkPositionInPartition.x,checkPositionInPartition.y];
            if (downId == null || !downId.Equals(centerFluidTile.id)) {
                return false;
            }
            int downFill = checkFluidPartitionData.fill[checkPositionInPartition.x,checkPositionInPartition.y];
            if (downFill >= FluidTileHelper.MaxFill) {
                return false;
            }
            int centerFill = centerPartitionData.fill[centerPositionInPartition.x,centerPositionInPartition.y];
            downFill += centerFill;
            if (downFill > FluidTileHelper.MaxFill) {
                centerFill = downFill-FluidTileHelper.MaxFill;
                downFill = FluidTileHelper.MaxFill;
            }
            if (centerFill < 0) {
                Debug.LogWarning("Fluid Merging for Below resulted in centerFill with fill less than 0");
            }
            if (centerFill <= 0) {
                centerPartitionData.ids[centerPositionInPartition.x,centerPositionInPartition.y] = null;
                setTile(position.x,position.y,null);
            } else {
                setTile(position.x,position.y,centerFluidTile);
            }
            setTile(checkPosition.x,checkPosition.y,ItemRegistry.getInstance().getFluidTileItem(downId));
            addFluidUpdate(checkPosition);
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
    }
}

