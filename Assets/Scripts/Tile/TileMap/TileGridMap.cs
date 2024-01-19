using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


/**
Takes in a 16 x 16 array of tileIDs and creates a TileMap out of them
**/
public class TileGridMap : AbstractTileMap<TileItem,TileData>
{    
    protected override TileData initTileData(TileItem tileItem) {
        /*
        if (id >= 0) {
            tileData = IdDataMap.getInstance().copyTileData(id);
            if (tileData.tileOptions.containsKey("rotation")) {
                tileData.tileOptions.set("rotation",devMode.rotation);
            }
        }
        */
        return null;
        //return tileData;
    }

    protected override void spawnItemEntity(TileItem tileItem, Vector2Int hitTilePosition, Vector2 worldPosition) {
        GameObject chunk = ChunkHelper.snapChunk(worldPosition.x,worldPosition.y);
        Transform entityContainer = Global.findChild(chunk.transform, "Entities").transform;    

        float realXPosition = transform.position.x+ hitTilePosition.x/2f+0.25f;
        float realYPosition = transform.position.y+ hitTilePosition.y/2f+0.25f;

        Vector2 spriteSize =  Global.getSpriteSize(tileItem.sprite);
        if (PlaceTile.mod(spriteSize.x,2) == 0) {
            realXPosition += 0.25f;
        }
        if (PlaceTile.mod(spriteSize.y,2) == 0) {
            realYPosition += 0.25f;
        }
        ItemSlot itemSlot = new ItemSlot(tileItem,1,new Dictionary<ItemSlotOption, object>());
        ItemEntityHelper.spawnItemEntity(new Vector3(realXPosition,realYPosition,0),itemSlot,entityContainer);
    }

    protected override Vector2Int getHitTilePosition(Vector2 position)
    {
        Vector2Int hitPosition = Global.Vector3IntToVector2Int(tilemap.WorldToCell(position));
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
        // Mathematically impossible to ever get here if maxSearchDist is infinity.
        // Since the biggest tile I'm probably ever gonna put in the game is 16x16, will never get here.
        Debug.LogError("FindTileAtLocation reached impossible to reach code. Something has gone very wrong!");
        return new Vector2Int(2147483647,2147483647);
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
        Vector2Int chunkPosition = getChunk(position);
        GameObject chunk = ChunkHelper.snapChunk(position.x/2,position.y/2);
        Vector2Int tilePositionInChunk = getTilePosition(position);
        Transform tileEntityContainer = Global.findChild(chunk.transform, "TileEntities").transform;
        deleteTileEntity(tileEntityContainer,new Vector3Int(tilePositionInChunk.x,tilePositionInChunk.y,0));

        tilemap.SetTile(new Vector3Int(position.x,position.y,0), null);
        dimensionChunkData[chunkPosition].data[tilePositionInChunk.x][tilePositionInChunk.y] = null;
    }

    protected override bool hitHardness(TileData tileData) {
        if (!tileData.options.ContainsKey(TileItemOption.Hardness)) { // uninteractable
            return false;
        }
        int hardness = Convert.ToInt32(tileData.options[TileItemOption.Hardness]) -1;
        tileData.options[TileItemOption.Hardness] = hardness;
        return hardness == 0;
    }

    protected override void setTile(int x, int y,TileData tileData) {
        if (tileData != null) {
            tilemap.SetTile(new Vector3Int(x,y,0),TileFactory.generateTile(tileData));
        }
        
    }

    private bool deleteTileEntity(Transform tileEntityContainer, Vector3Int position) {
        GameObject tileEntity = TileEntityHelper.getTileEntity(tileEntityContainer,gameObject.name,Global.Vector3IntToVector2Int(position));
        if (tileEntity != null) {
            GameObject.Destroy(tileEntity);
            return true;
        }
        return false;
    }
    public List<List<Dictionary<string,object>>> getSeralizedTileOptions(Vector2Int chunkPosition) {
        ChunkData<TileData> chunkData = dimensionChunkData[chunkPosition];
        List<List<Dictionary<string,object>>> nestedTileOptionList = new List<List<Dictionary<string, object>>>();
        for (int xIter = 0; xIter < 16; xIter ++) {
            List<Dictionary<string,object>> tileOptionList = new List<Dictionary<string,object>>();
            for (int yIter = 0; yIter < 16; yIter ++) {
                TileData tileData = chunkData.data[xIter][yIter];
                if (tileData == null) {
                    tileOptionList.Add(new Dictionary<string, object>());
                    continue;
                }
                Dictionary<string,object> serialized = new Dictionary<string, object>();
                 
                foreach (TileItemOption tileItemOption in tileData.options.Keys) {
                    if (TileEntityOptionFactory.isSerizable(tileItemOption)) {
                        TileEntityOptionFactory.serializeOption(tileItemOption,tileData.options[tileItemOption],serialized);
                    }
                }
                tileOptionList.Add(serialized);
            }
            nestedTileOptionList.Add(tileOptionList);
        }
        
        return nestedTileOptionList;
    }
}














