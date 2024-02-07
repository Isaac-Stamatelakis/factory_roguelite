using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


public class TileGridMap : AbstractTileMap<TileItem,TileData>
{    
    
    protected override void spawnItemEntity(TileItem tileItem, Vector2Int hitTilePosition, Vector2 worldPosition) {
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
        IChunk chunk = getChunk(position);
        Vector2Int partitionPosition = getPartitionPosition(position);
        Vector2Int tilePartitionPosition = position-partitionPosition*Global.ChunkPartitionSize;

        TileData tileData = getIdDataInChunk(position);
        if (tileData == null) {
            return;
        }
        if (((TileItem) tileData.getItemObject()).tileEntity != null) {
            Vector2Int partitionPositionInChunk = partitionPosition -chunk.getPosition()*Global.PartitionsPerChunk;
            IChunkPartition chunkPartition = chunk.getPartition(partitionPositionInChunk);
            TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(type);
            chunkPartition.removeTileEntity(layer,tilePartitionPosition);
        }
        tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
        if (partitions.ContainsKey(partitionPosition)) {
            partitions[partitionPosition][tilePartitionPosition.x,tilePartitionPosition.y] = null;
        }
        
    }

    protected override bool hitHardness(TileData tileData) {
        if (tileData == null) {
            return false;
        }
        if (!tileData.options.ContainsKey(TileItemOption.Hardness)) { // uninteractable
            return false;
        }
        int hardness = Convert.ToInt32(tileData.options[TileItemOption.Hardness]) -1;
        tileData.options[TileItemOption.Hardness] = hardness;
        return hardness == 0;
    }

    protected override void setTile(int x, int y,TileData tileData) {
        TileBase tileBase = ((TileItem) tileData.getItemObject()).tile;
        if (tileData != null) {
            tilemap.SetTile(new Vector3Int(x,y,0),tileBase);
        } else {
            tilemap.SetTile(new Vector3Int(x,y,0),null);
        }
    }

    public override void initPartition(Vector2Int partitionPosition)
    {
        partitions[partitionPosition] = new TileData[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
    }
}














