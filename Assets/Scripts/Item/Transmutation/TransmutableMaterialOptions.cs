using System.Collections;
using System.Collections.Generic;
using Item.Transmutation;
using Item.Transmutation.Items;
using Tiles.Fluid;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Items.Transmutable
{
    public enum TransmutableMaterialState
    {
        None,
        Metal,
        Gem,
    }
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Options")]
    public class TransmutableMaterialOptions : ScriptableObject
    {
        public TransmutableMaterialState transmutableMaterialState;
        public TransmutableItemState BaseState = TransmutableItemState.Ingot;
        public List<TransmutableStateOptions> States;
        public List<TransmutableTileStateOptions> TileStates;
        public List<TransmutableFluidTileOptions> FluidStates;
    }
    
    [System.Serializable]
    public class TransmutableStateOptions {
        public TransmutableItemObjectState state;
        public Sprite[] sprites;
    }

    [System.Serializable]
    public class TransmutableTileStateOptions
    {
        public TransmutableTileItemState state;
        public TileBase tile;
    }
    
    [System.Serializable]
    public class TransmutableFluidTileOptions
    {
        public TransmutableFluidItemState state;
        public FluidTile packedTile;
        public FluidTile unpackedTile;
        public float opacity = 1;
        public float damage;
        public int viscosity = 5;
        public bool lit;
    }
}

