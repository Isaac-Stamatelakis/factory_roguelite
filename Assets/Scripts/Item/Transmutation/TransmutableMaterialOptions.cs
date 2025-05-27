using System.Collections;
using System.Collections.Generic;
using Item.Transmutation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items.Transmutable
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Options")]
    public class TransmutableMaterialOptions : ScriptableObject
    {
        public TransmutableItemState BaseState = TransmutableItemState.Ingot;
        public List<TransmutableStateOptions> States;
    }
    
    [System.Serializable]
    public class TransmutableStateOptions {
        public TransmutableItemState state;
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
        public FluidTileItem tile;
    }
}

