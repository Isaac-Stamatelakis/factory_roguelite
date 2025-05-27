using System.Collections;
using System.Collections.Generic;
using Item.Transmutation;
using Item.Transmutation.Items;
using Tiles.Fluid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items.Transmutable
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Options")]
    public class TransmutableMaterialOptions : ScriptableObject
    {
        public TransmutableItemState BaseState = TransmutableItemState.Ingot;
        public List<TransmutableStateOptions> States;
        public List<TransmutableTileStateOptions> TileStates;
        public List<TransmutableFluidItemState> FluidStates;
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
        public TransmutableTileItemState state;
        public FluidTile tile;
        public float opacity;
    }
}

