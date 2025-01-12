using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Tiles;

/// <summary>
/// Checks tiles next 
/// </summary>
public class BackgroundRuleTile : RuleTile, IIDTile
{
    public string id;
    public string getId()
    {
        return id;
    }

    public override bool RuleMatch(int neighbor, TileBase other)
        {
            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This: return other != null;
                case TilingRuleOutput.Neighbor.NotThis: return other == null;
            }
            return true;
        }

    public void setID(string id)
    {
        this.id = id;
    }
}
