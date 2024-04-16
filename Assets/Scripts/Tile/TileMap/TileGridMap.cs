using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using ChunkModule;
using TileEntityModule;
using TileMapModule.Layer;
using TileMapModule.Type;
using TileMapModule.Place;
using ChunkModule.PartitionModule;
using ChunkModule.ClosedChunkSystemModule;
using Tiles;
using ItemModule;

namespace TileMapModule {
    public class TileGridMap : AbstractTileMap<TileItem>
    {   
        protected override void spawnItemEntity(ItemObject itemObject, int amount, Vector2Int hitTilePosition) {
            ILoadedChunk chunk = getChunk(hitTilePosition);  

            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;

            Vector2 spriteSize =  Global.getSpriteSize(itemObject.getSprite());
            if (PlaceTile.mod(spriteSize.x,2) == 0) {
                realXPosition += 0.25f;
            }
            if (PlaceTile.mod(spriteSize.y,2) == 0) {
                realYPosition += 0.25f;
            }
            ItemSlot itemSlot = ItemSlotFactory.createNewItemSlot(itemObject,amount);
            ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,chunk.getEntityContainer());
        }

        public TileOptions getOptionsAtPosition(Vector2Int realTilePosition) {
            if (realTilePosition == new Vector2Int(-2147483647,-2147483647)) {
                return null;
            }
            IChunkPartition partition = getPartitionAtPosition(realTilePosition);
            Vector2Int tilePositionInPartition = base.getTilePositionInPartition(realTilePosition);
            return partition.getTileOptions(tilePositionInPartition);
        }
        protected override Vector2Int getHitTilePosition(Vector2 position)
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
        protected override void breakTile(Vector2Int position) {
            IChunkPartition partition = getPartitionAtPosition(position);
            if (partition == null) {
                return;
            }
            Vector2Int tilePositionInPartition = getTilePositionInPartition(position);
            TileEntity tileEntity = getTileEntityAtPosition(position);
            if (tileEntity != null) {
                TileMapLayer layer = type.toLayer();
                partition.breakTileEntity(layer,tilePositionInPartition);
                deleteTileEntityFromConduit(position);
            }
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
            writeTile(partition,tilePositionInPartition,null);
            TileHelper.tilePlaceTileEntityUpdate(position, null,this);
            callListeners(position);
        }

        public TileEntity getTileEntityAtPosition(Vector2Int position) {
            IChunkPartition partition = getPartitionAtPosition(position);
            if (partition == null) {
                return null;
            }
            Vector2Int tilePositionInPartition = getTilePositionInPartition(position);
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

        protected override void setTile(int x, int y,TileItem tileItem) {
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
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(x,y));
                
                if (tileOptions.SerializedTileOptions.mirror) {
                    transformMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0f, 180f, tileOptions.SerializedTileOptions.rotation), Vector3.one);
                } else {
                    transformMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, tileOptions.SerializedTileOptions.rotation), Vector3.one);
                }
                tilemap.SetTransformMatrix(new Vector3Int(x,y,0), transformMatrix);
            }
            
        }

        public override void hitTile(Vector2 position) {
            Vector2Int hitTilePosition = getHitTilePosition(position);
            Vector3Int vec3Hit = (Vector3Int) hitTilePosition;
            TileOptions tileOptions = getOptionsAtPosition(hitTilePosition);
            if (hitHardness(hitTilePosition)) {
                TileItem tileItem = getTileItem(hitTilePosition);
                if (tileItem.tileOptions == null || tileItem.tileOptions.StaticOptions == null || tileItem.tileOptions.StaticOptions.dropOptions.Count == 0) {
                    spawnItemEntity(tileItem,1,hitTilePosition);
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
                                int amount = UnityEngine.Random.Range(dropOption.lowerAmount,dropOption.upperAmount+1);
                                amount = Mathf.Max(1,amount);
                                spawnItemEntity(dropOption.itemObject,amount,hitTilePosition);
                            }
                            
                        }
                    }
                }
                
                breakTile(hitTilePosition);
            }
        }

        protected override void writeTile(IChunkPartition partition, Vector2Int position, TileItem item)
        {
            if (partition == null) {
                return;
            }
            partition.setTile(position,getType().toLayer(),item);
        }

        protected TileItem getTileItem(Vector2Int cellPosition) {
            IChunkPartition partition = getPartitionAtPosition(cellPosition);
            Vector2Int positionInPartition = getTilePositionInPartition(cellPosition);
            TileItem tileItem = partition.GetTileItem(positionInPartition,getType().toLayer());
            return tileItem;
        }
    }
}
















