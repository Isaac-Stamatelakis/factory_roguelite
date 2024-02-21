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

namespace TileMapModule {
    public class TileGridMap : AbstractTileMap<TileItem>
    {   
        protected override void spawnItemEntity(TileItem tileItem, Vector2Int hitTilePosition) {
            IChunk chunk = getChunk(hitTilePosition);  

            float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
            float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;

            Vector2 spriteSize =  Global.getSpriteSize(tileItem.getSprite());
            if (PlaceTile.mod(spriteSize.x,2) == 0) {
                realXPosition += 0.25f;
            }
            if (PlaceTile.mod(spriteSize.y,2) == 0) {
                realYPosition += 0.25f;
            }
            ItemSlot itemSlot = new ItemSlot(tileItem,1,new Dictionary<string, object>());
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
            Vector2Int tilePositionInPartition = getTilePositionInPartition(position);
            TileEntity tileEntity = partition.GetTileEntity(tilePositionInPartition);

            if (tileEntity != null) {
                TileMapLayer layer = type.toLayer();
                partition.breakTileEntity(layer,tilePositionInPartition);
                deleteTileEntityFromConduit(position);
            }
            tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
            writeTile(partition,tilePositionInPartition,null);
        }

        protected void deleteTileEntityFromConduit(Vector2Int position) {
            if (base.closedChunkSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem) {
                    conduitTileClosedChunkSystem.tileEntityDeleteUpdate(position);
            }
        }

        protected override bool hitHardness(TileOptions tileOptions) {
            if (tileOptions == null) {
                return false;
            }
            if (!tileOptions.Options.ContainsKey(TileOption.Hardness)) { // uninteractable
                return false;
            }
            int hardness = Convert.ToInt32(tileOptions.Options[TileOption.Hardness]) -1;
            tileOptions.Options[TileOption.Hardness] = hardness;
            return hardness == 0;
        }

        protected override void setTile(int x, int y,TileItem tileItem) {
            TileBase tileBase = tileItem.tile;
            tilemap.SetTile(new Vector3Int(x,y,0),tileBase);
        }

        public override void hitTile(Vector2 position) {
            Vector2Int hitTilePosition = getHitTilePosition(position);
            TileOptions tileOptions = getOptionsAtPosition(hitTilePosition);
            if (hitHardness(tileOptions)) {
                IChunkPartition partition = getPartitionAtPosition(hitTilePosition);
                Vector2Int positionInPartition = getTilePositionInPartition(hitTilePosition);
                TileItem tileItem = partition.GetTileItem(positionInPartition,getType().toLayer());
                spawnItemEntity(tileItem,hitTilePosition);
                breakTile(hitTilePosition);
            }
        }

        protected override void writeTile(IChunkPartition partition, Vector2Int position, TileItem item)
        {
            partition.setTile(position,getType().toLayer(),item);
        }
    }
}
















