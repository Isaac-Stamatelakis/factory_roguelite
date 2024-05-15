using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Transmutable {
    public class TransmutableItemObject : PresetItemObject, ITransmutableItem, IStateItem
    {
        [SerializeField] private TransmutableItemState state;
        [SerializeField] private TransmutableItemMaterial material;
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

        public ItemState getItemState()
        {
            return state.getMatterState();
        }
        public void setSprite(Sprite sprite) {
            this.sprites = new Sprite[]{sprite};
        }
    }
}

