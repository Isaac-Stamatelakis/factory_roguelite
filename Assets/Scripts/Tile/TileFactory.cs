using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TileFactory
{
    public static TileBase generateTile(TileData tileData) {
        Dictionary<TileItemOption, object> options = tileData.options;
        int chisel = 0;
        if (options.ContainsKey(TileItemOption.Chisel)) {
            chisel = Convert.ToInt32(options[TileItemOption.Chisel]);
        }
        if (options.ContainsKey(TileItemOption.RuleTile)) {
            if (chisel >= 0) {
                return (RuleTile) options[TileItemOption.RuleTile]; // TODO Add chisel functionality
            } else {
                return (RuleTile) options[TileItemOption.RuleTile];
            }
        } else if (options.ContainsKey(TileItemOption.AnimatedTile)) {
            CustomizableAnimatedTile customizableAnimatedTile = ScriptableObject.CreateInstance<CustomizableAnimatedTile>();
            AnimatedTile animatedTile;
            if (chisel >= 0) {
                animatedTile = (AnimatedTile) options[TileItemOption.AnimatedTile]; // TODO Add chisel functionality
            } else {
                animatedTile = (AnimatedTile) options[TileItemOption.AnimatedTile];
            }
            customizableAnimatedTile.m_AnimatedSprites = animatedTile.m_AnimatedSprites;
            customizableAnimatedTile.m_AnimationStartFrame = animatedTile.m_AnimationStartFrame;
            customizableAnimatedTile.m_AnimationStartTime = animatedTile.m_AnimationStartTime;
            customizableAnimatedTile.m_MaxSpeed=animatedTile.m_MaxSpeed;
            customizableAnimatedTile.m_MinSpeed = animatedTile.m_MinSpeed;
            customizableAnimatedTile.m_TileColliderType = animatedTile.m_TileColliderType;
            if (options.ContainsKey(TileItemOption.Rotation)) {
                customizableAnimatedTile.rotation = Convert.ToInt32(options[TileItemOption.Rotation]);
            }
            return customizableAnimatedTile; 
        }
    
        CustomizableTile tile = ScriptableObject.CreateInstance<CustomizableTile>();
        if (chisel >= 0) {
            tile.sprite = tileData.itemObject.sprite; // TODO Add chisel functionality
        } else {
            tile.sprite = tileData.itemObject.sprite;
        }
        if (options.ContainsKey(TileItemOption.Rotation)) {
            tile.rotation = Convert.ToInt32(options[TileItemOption.Rotation]);
        }
        return tile;
    }

}
