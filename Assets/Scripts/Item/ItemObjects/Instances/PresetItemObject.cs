using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Tags;

namespace Items {
    public abstract class PresetItemObject : ItemObject
    {
        [SerializeField] protected Sprite[] sprites;
        [SerializeField] protected ItemDisplayType displayType;
        public override ItemDisplayType? getDisplayType()
        {
            return displayType;
        }

        public override Sprite getSprite()
        {
            if (sprites == null || sprites.Length < 1) {
                return null;
            }
            return sprites[0];
        }

        public override Sprite[] getSprites()
        {
            return sprites;
        }
    }
}

