using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.Slot;
using UnityEngine;

namespace Items.Transmutable {
    public class TransmutableItemObject : ItemObject, ITransmutableItem, IStateItem
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
        public override Sprite[] getSprites()
        {
            return material.GetOptionStateDict()[state].sprites;
        }

        public override Sprite getSprite()
        {
            return getSprites()[0];
        }

        public override ItemDisplayType? getDisplayType()
        {
            var sprites = getSprites();
            return sprites?.Length > 1 ? ItemDisplayType.Animated : ItemDisplayType.Single;
        }

        public override GameStageObject GetGameStageObject()
        {
            return material?.gameStageObject;
        }
    }
}

