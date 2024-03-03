using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Transmutable {
    public class TransmutableItemObject : ItemObject, ITransmutableItem, IStateItem
    {
        protected TransmutableItemState state;
        protected TransmutableItemMaterial material;
        public Sprite sprite;

        public TransmutableItemMaterial getMaterial()
        {
            return material;
        }

        public TransmutableItemState getState()
        {
            return state;
        }

        public void setState(TransmutableItemState state)
        {
            this.state = state;
        }

        public void setMaterial(TransmutableItemMaterial material)
        {
            this.material = material;
        }

        public override Sprite getSprite()
        {
            return sprite;
        }

        public ItemState getItemState()
        {
            return state.getMatterState();
        }
    }
}

