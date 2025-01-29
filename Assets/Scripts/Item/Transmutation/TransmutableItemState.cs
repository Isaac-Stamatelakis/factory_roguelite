using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Items.Transmutable;

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
    Poor_Gem,
    Ore
}

public static class TransmutableItemStateExtension {
    public static string GetPrefix(this TransmutableItemState state)
    {
        return state switch
        {
            TransmutableItemState.Ingot => "",
            TransmutableItemState.Dust => "",
            TransmutableItemState.Plate => "",
            TransmutableItemState.Wire => "",
            TransmutableItemState.Block => "",
            TransmutableItemState.Fine_Wire => "Fine",
            TransmutableItemState.Double_Plate => "Double",
            TransmutableItemState.Small_Dust => "Small",
            TransmutableItemState.Tiny_Dust => "Tiny",
            TransmutableItemState.Rod => "",
            TransmutableItemState.Bolt => "",
            TransmutableItemState.Screw => "",
            TransmutableItemState.Liquid => "",
            TransmutableItemState.Gas => "",
            TransmutableItemState.Plasma => "",
            TransmutableItemState.Magnificent_Gem => "Magnificent",
            TransmutableItemState.Exceptional_Gem => "Exceptional",
            TransmutableItemState.Gem => "",
            TransmutableItemState.Mediocre_Gem => "Mediocre",
            TransmutableItemState.Poor_Gem => "Poor",
            TransmutableItemState.Ore => "",
            _ => ""
        };
    }

    public static string GetSuffix(this TransmutableItemState state)
    {
        return state switch
        {
            TransmutableItemState.Ingot => "Ingot",
            TransmutableItemState.Dust => "Dust",
            TransmutableItemState.Plate => "Plate",
            TransmutableItemState.Wire => "Wire",
            TransmutableItemState.Block => "Block",
            TransmutableItemState.Fine_Wire => "Wire",
            TransmutableItemState.Double_Plate => "Plate",
            TransmutableItemState.Small_Dust => "Dust",
            TransmutableItemState.Tiny_Dust => "Dust",
            TransmutableItemState.Rod => "Rod",
            TransmutableItemState.Bolt => "Bolt",
            TransmutableItemState.Screw => "Screw",
            TransmutableItemState.Liquid => "Liquid",
            TransmutableItemState.Gas => "Gas",
            TransmutableItemState.Plasma => "Plasma",
            TransmutableItemState.Magnificent_Gem => "",
            TransmutableItemState.Exceptional_Gem => "",
            TransmutableItemState.Gem => "",
            TransmutableItemState.Mediocre_Gem => "",
            TransmutableItemState.Poor_Gem => "",
            TransmutableItemState.Ore => "Ore",
            _ => ""
        };
    }
    /// <summary>
    /// Returns the ratio of the state in terms of ingot.
    /// <summary>
    public static float getRatio(this TransmutableItemState state) {
        // TODO invert this
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
            case TransmutableItemState.Ore:
                return 1 / 2f;
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
}