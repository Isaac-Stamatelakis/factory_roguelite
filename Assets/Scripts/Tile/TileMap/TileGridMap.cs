using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Chunks;
using TileEntity;
using TileMaps.Layer;
using TileMaps.Type;
using TileMaps.Place;
using Chunks.Partitions;
using Chunks.Systems;
using Tiles;
using Items;
using Entities;

namespace TileMaps {
    public interface ITileGridMap {
        public TileItem getTileItem(Vector2Int cellPosition);
    }
    public class TileGridMap : AbstractTileMap<TileItem>, ITileGridMap
    {   
        protected override void SpawnItemEntity(ItemObject itemObject, uint amount, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = GetChunk(hitTilePosition);  

            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;
            Sprite[] itemSprites = itemObject.getSprites();
            if (itemSprites.Length == 0) {
                Debug.LogError("Tried to spawn item with no sprite");
                return;
            }
            Vector2 spriteSize =  Global.getSpriteSize(itemSprites[0]);
            if (PlaceTile.mod(spriteSize.x,2) == 0) {
                realXPosition += 0.25f;
            }
            if (PlaceTile.mod(spriteSize.y,2) == 0) {
                realYPosition += 0.25f;
            }
            ItemSlot itemSlot = ItemSlotFactory.CreateNewItemSlot(itemObject,amount);
            ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }

        public TileOptions getOptionsAtPosition(Vector2Int realTilePosition) {
            if (realTilePosition == new Vector2Int(-2147483647,-2147483647)) {
                return null;
            }
            IChunkPartition partition = GetPartitionAtPosition(realTilePosition);
            Vector2Int tilePositionInPartition = base.GetTilePositionInPartition(realTilePosition);
            return partition.getTileOptions(tilePositionInPartition);
        }
        protected override Vector2Int GetHitTilePosition(Vector2 position)
        {
            Vector2Int hitPosition = worldToTileMapPosition(position);
            int maxSearchWidth = 16;
            int searchWidth = 1;
            while (searchWidth < maxSearchWidth) {
                if (Global.modInt(searchWidth,2) == 0) {
                    for (int x = searchWidth/2-1; x >= -searchWidth/2; x --) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y-(searchWidth/2),0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+x,hitPosition.y-(searchWidth/2));
                        }
                    }
                    for (int y = -searchWidth/2+1; y <= searchWidth/2-1; y ++) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x-(searchWidth/2),hitPosition.y+y,0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x-(searchWidth/2), hitPosition.y+y);
                        }
                    }
                } else {
                    for (int x = -(searchWidth-1)/2; x <= (searchWidth-1)/2; x ++) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2,0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2);
                        }
                    }
                    for (int y = (searchWidth-1)/2-1; y >= -(searchWidth-1)/2; y --) {
                        TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+(searchWidth-1)/2,hitPosition.y+y,0));
                        if (isHitTile(tileBase,searchWidth)) {
                            return new Vector2Int(hitPosition.x+(searchWidth-1)/2, hitPosition.y+y);
                        }
                    }
                }
                searchWidth ++;
            }
            return new Vector2Int(-2147483647,-2147483647);
        }
        private bool isHitTile(TileBase tileBase, int searchWidth) {
            int spriteY = 0;
            if (tileBase is Tile) {
                spriteY = (int) Global.getSpriteSize(((Tile) tileBase).sprite).y;
            } else if (tileBase is RuleTile) {
                spriteY = (int) Global.getSpriteSize(((RuleTile) tileBase).m_DefaultSprite).y;
            } else if (tileBase is AnimatedTile) {
                spriteY = (int) Global.getSpriteSize(((AnimatedTile) tileBase).m_AnimatedSprites[0]).y;
            }
            return spriteY >= searchWidth;
        } 
        protected override void BreakTile(Vector2Int position) {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) {
                return;
            }
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            ITileEntityInstance tileEntity = getTileEntityAtPosition(position);
            if (tileEntity != null) {
                TileMapLayer layer = type.toLayer();
                partition.breakTileEntity(layer,tilePositionInPartition);
                deleteTileEntityFromConduit(position);
            }
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
            WriteTile(partition,tilePositionInPartition,null);
            TileHelper.tilePlaceTileEntityUpdate(position, null,this);
            CallListeners(position);
        }

        public ITileEntityInstance getTileEntityAtPosition(Vector2Int position) {
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) {
                return null;
            }
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(position);
            return partition.GetTileEntity(tilePositionInPartition);
        }
        protected void deleteTileEntityFromConduit(Vector2Int position) {
            if (base.closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                conduitTileClosedChunkSystem.tileEntityDeleteUpdate(position);
            }
        }

        protected bool hitHardness(Vector2Int cellPosition) {
            TileOptions tileOptions = getOptionsAtPosition(cellPosition);
            if (tileOptions == null) {
                return false;
            }
            if (!tileOptions.StaticOptions.hitable) { // uninteractable
                return false;
            }
            TileItem tileItem = getTileItem(cellPosition);
            DynamicTileOptions dynamicTileOptions = tileOptions.DynamicTileOptions;
            dynamicTileOptions.hardness--;
            tileOptions.DynamicTileOptions = dynamicTileOptions;
            bool broken = dynamicTileOptions.hardness == 0;
            if (tileItem.tile is Tile tile && tile.colliderType == Tile.ColliderType.Grid) {
                if (!broken) {
                    float breakRatio = 1f-((float)dynamicTileOptions.hardness)/tileItem.tileOptions.DynamicTileOptions.hardness;
                    closedChunkSystem.BreakIndicator.setBreak(breakRatio,cellPosition);
                } else {
                    closedChunkSystem.BreakIndicator.removeBreak(cellPosition);
                }
            }
            return broken;
        }

        protected override void SetTile(int x, int y,TileItem tileItem) {
            TileBase tileBase = tileItem.tile;
            if (tileBase == null) {
                return;
            }
            if (tileBase is IStateTile stateTile) {
                TileOptions tileOptions = getOptionsAtPosition(new Vector2Int(x,y));
                Vector2 pos = new Vector2(x/2f+0.25f,y/2f+0.25f);
                tileBase = stateTile.getTileAtState(tileOptions.SerializedTileOptions.state);
            } 
            tilemap.SetTile(new Vector3Int(x,y,0),tileBase);
            if (tileItem.tileOptions != null && tileItem.tileOptions.StaticOptions != null && tileItem.tileOptions.StaticOptions.rotatable) {
                TileOptions tileOptions = getOptionsAtPosition(new Vector2Int(x,y));
                
                if (tileBase is IStateRotationTile stateRotationTile) {
                    tilemap.SetTile(
                        new Vector3Int(x,y,0), 
                        stateRotationTile.getTile(tileOptions.SerializedTileOptions.rotation,tileOptions.SerializedTileOptions.mirror)
                    );
                } else {
                    Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(x,y));
                    if (tileOptions.SerializedTileOptions.mirror) {
                        transformMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0f, 180f, tileOptions.SerializedTileOptions.rotation), Vector3.one);
                    } else {
                        transformMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, tileOptions.SerializedTileOptions.rotation), Vector3.one);
                    }
                    tilemap.SetTransformMatrix(new Vector3Int(x,y,0), transformMatrix);
                }
                
            }
            
        }

        public override void hitTile(Vector2 position) {
            Vector2Int hitTilePosition = GetHitTilePosition(position);
            Vector3Int vec3Hit = (Vector3Int) hitTilePosition;
            TileOptions tileOptions = getOptionsAtPosition(hitTilePosition);
            if (hitHardness(hitTilePosition)) {
                TileItem tileItem = getTileItem(hitTilePosition);
                if (tileItem.tileOptions == null || tileItem.tileOptions.StaticOptions == null || tileItem.tileOptions.StaticOptions.dropOptions.Count == 0) {
                    SpawnItemEntity(tileItem,1,hitTilePosition);
                } else {
                    int totalWeight = 0;
                    foreach (DropOption dropOption in tileItem.tileOptions.StaticOptions.dropOptions) {
                        totalWeight += dropOption.weight;
                    }
                    int ran = UnityEngine.Random.Range(0,totalWeight);
                    totalWeight = 0;
                    foreach (DropOption dropOption in tileItem.tileOptions.StaticOptions.dropOptions) {
                        totalWeight += dropOption.weight;
                        if (totalWeight >= ran) {
                            if (dropOption.itemObject != null) {
                                uint amount = (uint)UnityEngine.Random.Range(dropOption.lowerAmount,dropOption.upperAmount+1);
                                amount = GlobalHelper.MaxUInt(1, amount);
                                SpawnItemEntity(dropOption.itemObject,amount,hitTilePosition);
                            }
                            
                        }
                    }
                }
                
                BreakTile(hitTilePosition);
            }
        }

        protected override void WriteTile(IChunkPartition partition, Vector2Int position, TileItem item)
        {
            if (partition == null) {
                return;
            }
            partition.setTile(position,getType().toLayer(),item);
        }

        public TileItem getTileItem(Vector2Int cellPosition) {
            IChunkPartition partition = GetPartitionAtPosition(cellPosition);
            if (partition == null) {
                return null;
            }
            Vector2Int positionInPartition = GetTilePositionInPartition(cellPosition);
            TileItem tileItem = partition.GetTileItem(positionInPartition,getType().toLayer());
            return tileItem;
        }
    }
}
















