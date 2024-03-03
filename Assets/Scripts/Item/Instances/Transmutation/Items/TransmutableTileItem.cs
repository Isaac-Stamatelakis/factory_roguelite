using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Transmutable {
    public class TransmutableTileItem : TileItem, ITransmutableItem
    {
        protected TransmutableItemState state;
        protected TransmutableItemMaterial material;
        public TransmutableItemMaterial getMaterial()
        {
            return material;
        }

        public TransmutableItemState getState()
        {
            return state;
        }

        public void setMaterial(TransmutableItemMaterial material)
        {
            this.material = material;
        }

        public void setState(TransmutableItemState state)
        {
            this.state = state;
        }
    }
}

