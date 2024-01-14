using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TileFactory
{
    public static TileBase generateTile(TileData tileData) {
        if (tileData.tileOptions == null) {
            CustomizableTile nullTile = ScriptableObject.CreateInstance<CustomizableTile>();
            nullTile.sprite = IdDataMap.getInstance().GetSprite(tileData.id);
            return nullTile;
        };
        int chisel = -1;
        if (tileData.tileOptions.containsKey("chisel")) {
            chisel = Convert.ToInt32(tileData.tileOptions.get("chisel"));
        }
        if (tileData.tileOptions.containsKey("ruletile")) { // ruletile overides animated.
            if (chisel >= 0) {
                return Resources.Load<RuleTile>((string) tileData.tileOptions.get("ruletile") + chisel.ToString());
            } else {
                return Resources.Load<RuleTile>((string) tileData.tileOptions.get("ruletile"));
            }
        }
        if (tileData.tileOptions.containsKey("animated")) {
            CustomizableAnimatedTile customizableAnimatedTile = ScriptableObject.CreateInstance<CustomizableAnimatedTile>();
            AnimatedTile animatedTile;
            if (chisel >= 0) {
                animatedTile = Resources.Load<AnimatedTile>((string) tileData.tileOptions.get("animated") + chisel.ToString());
            } else {
                animatedTile = Resources.Load<AnimatedTile>((string) tileData.tileOptions.get("animated"));
            }
            customizableAnimatedTile.m_AnimatedSprites = animatedTile.m_AnimatedSprites;
            customizableAnimatedTile.m_AnimationStartFrame = animatedTile.m_AnimationStartFrame;
            customizableAnimatedTile.m_AnimationStartTime = animatedTile.m_AnimationStartTime;
            customizableAnimatedTile.m_MaxSpeed=animatedTile.m_MaxSpeed;
            customizableAnimatedTile.m_MinSpeed = animatedTile.m_MinSpeed;
            customizableAnimatedTile.m_TileColliderType = animatedTile.m_TileColliderType;
            if (tileData.tileOptions.containsKey("rotation")) {
                customizableAnimatedTile.rotation = Convert.ToInt32(tileData.tileOptions.get("rotation"));
            }
            return customizableAnimatedTile; 
        }
        
        CustomizableTile tile = ScriptableObject.CreateInstance<CustomizableTile>();
        if (chisel >= 0) {
            tile.sprite = Resources.Load<Sprite>(tileData.spritePath + chisel.ToString());
        } else {
            tile.sprite = IdDataMap.getInstance().GetSprite(tileData.id);
        }
        if (tileData.tileOptions.containsKey("rotation")) {
            tile.rotation = Convert.ToInt32(tileData.tileOptions.get("rotation"));
        }
        
        
        return tile;
    }

}
