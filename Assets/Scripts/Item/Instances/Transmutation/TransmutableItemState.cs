using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Transmutable;

public enum TransmutableItemState {
    Ingot,
    Dust,
    Plate,
    Wire,
    Block,
    Fine_Wire,
    Double_Plate,
    Small_Dust,
    Tiny_Dust,
    Rod,
    Bolt,
    Screw,
    Liquid,
    Gas,
    Plasma,
    Magnificent_Gem,
    Exceptional_Gem,
    Gem,
    Mediocre_Gem,
    Poor_Gem
}

public static class TransmutableItemStateExtension {
    public static string getPrefix(this TransmutableItemState state) {
        switch (state) {
            case TransmutableItemState.Ingot:
                return "";
            case TransmutableItemState.Dust:
                return "";
            case TransmutableItemState.Plate:
                return "";
            case TransmutableItemState.Wire:
                return "";
            case TransmutableItemState.Block:
                return "";
            case TransmutableItemState.Fine_Wire:
                return "Fine";
            case TransmutableItemState.Double_Plate:
                return "Double";
            case TransmutableItemState.Small_Dust:
                return "Small";
            case TransmutableItemState.Tiny_Dust:
                return "Tiny";
            case TransmutableItemState.Rod:
                return "";
            case TransmutableItemState.Bolt:
                return "";
            case TransmutableItemState.Screw:
                return "";
            case TransmutableItemState.Liquid:
                return "";
            case TransmutableItemState.Gas:
                return "";
            case TransmutableItemState.Plasma:
                return "";
            case TransmutableItemState.Magnificent_Gem:
                return "Magnificent";
            case TransmutableItemState.Exceptional_Gem:
                return "Exceptional";
            case TransmutableItemState.Gem:
                return "";
            case TransmutableItemState.Mediocre_Gem:
                return "Mediocre";
            case TransmutableItemState.Poor_Gem:
                return "Poor";
            default:
                return "";
        }
    }

    public static string getSuffix(this TransmutableItemState state) {
        switch (state) {
            case TransmutableItemState.Ingot:
                return "Ingot";
            case TransmutableItemState.Dust:
                return "Dust";
            case TransmutableItemState.Plate:
                return "Plate";
            case TransmutableItemState.Wire:
                return "Wire";
            case TransmutableItemState.Block:
                return "Block";
            case TransmutableItemState.Fine_Wire:
                return "Wire";
            case TransmutableItemState.Double_Plate:
                return "Plate";
            case TransmutableItemState.Small_Dust:
                return "Dust";
            case TransmutableItemState.Tiny_Dust:
                return "Dust";
            case TransmutableItemState.Rod:
                return "Rod";
            case TransmutableItemState.Bolt:
                return "Bolt";
            case TransmutableItemState.Screw:
                return "Screw";
            case TransmutableItemState.Liquid:
                return "Liquid";
            case TransmutableItemState.Gas:
                return "Gas";
            case TransmutableItemState.Plasma:
                return "Plasma";
            case TransmutableItemState.Magnificent_Gem:
                return "";
            case TransmutableItemState.Exceptional_Gem:
                return "";
            case TransmutableItemState.Gem:
                return "";
            case TransmutableItemState.Mediocre_Gem:
                return "";
            case TransmutableItemState.Poor_Gem:
                return "";
            default:
                return "";
        }
    }
    /// <summary>
    /// Returns the ratio of the state in terms of ingot.
    /// <summary>
    public static float getRatio(this TransmutableItemState state) {
        switch (state) {
            case TransmutableItemState.Ingot:
                return 1f;
            case TransmutableItemState.Dust:
                return 1f;
            case TransmutableItemState.Plate:
                return 1f;
            case TransmutableItemState.Wire:
                return 2f;
            case TransmutableItemState.Block:
                return 1/32f;
            case TransmutableItemState.Fine_Wire:
                return 8f;
            case TransmutableItemState.Double_Plate:
                return 1/2f;
            case TransmutableItemState.Small_Dust:
                return 16f;
            case TransmutableItemState.Tiny_Dust:
                return 4f;
            case TransmutableItemState.Rod:
                return 2f;
            case TransmutableItemState.Bolt:
                return 8f;
            case TransmutableItemState.Screw:
                return 8f;
            case TransmutableItemState.Liquid:
                return 200f;
            case TransmutableItemState.Gas:
                return 200f;
            case TransmutableItemState.Plasma:
                return 200f;
            case TransmutableItemState.Magnificent_Gem:
                return 1/16f;
            case TransmutableItemState.Exceptional_Gem:
                return 1/4f;
            case TransmutableItemState.Gem:
                return 1f;
            case TransmutableItemState.Mediocre_Gem:
                return 4f;
            case TransmutableItemState.Poor_Gem:
                return 16f;
            default:
                return 0;
        }
    }
    public static ItemState getMatterState(this TransmutableItemState state) {
        switch (state) {
            case TransmutableItemState.Liquid:
                return ItemState.Fluid;
            case TransmutableItemState.Gas:
                return ItemState.Fluid;
            case TransmutableItemState.Plasma:
                return ItemState.Fluid;
            default:
                return ItemState.Solid;
        }
    }

    public static float getComparedRatio(this TransmutableItemState state, TransmutableItemState compare) {
        return state.getRatio()/compare.getRatio();
    }

    
}

public class TransmutableItemSprites {
    private static Dictionary<TransmutableItemState, Sprite> dict = new Dictionary<TransmutableItemState, Sprite>();
    private static TransmutableItemSprites instance;
    private TransmutableItemSprites() {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Items/Main/TransmutableItems/TransmutableSprites");
        Debug.Log("Loaded " + sprites.Length + " Base Transmutation Sprites");
        foreach (TransmutableItemState state in (TransmutableItemState.GetValues(typeof(TransmutableItemState)))) {
            foreach (Sprite sprite in sprites) {
                if (sprite.name == state.ToString()) {
                    if (!dict.ContainsKey(state)) {
                        dict[state] = sprite;
                        break;
                    }
                }
            }
        }
    }
    public static TransmutableItemSprites getInstance() {
        if (instance == null) {
            instance = new TransmutableItemSprites();
        }
        return instance;
    }

    public Sprite getSprite(TransmutableItemState state) {
        if (dict.ContainsKey(state)) {
            return dict[state];
        }
        return null;
    }
}