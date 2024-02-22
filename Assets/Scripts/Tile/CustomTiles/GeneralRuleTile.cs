using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Checks tiles next 
/// </summary>
public class GeneralRuleTile : RuleTile
{
    public override bool RuleMatch(int neighbor, TileBase other)
        {
            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This: return other != null;
                case TilingRuleOutput.Neighbor.NotThis: return other == null;
            }
            return true;
        }
}
