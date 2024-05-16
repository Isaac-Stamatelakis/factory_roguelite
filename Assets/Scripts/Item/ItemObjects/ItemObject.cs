using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    public enum ItemDisplayType {
        Single,
        Stack,
        Animated
    }
    public abstract class ItemObject : ScriptableObject {
        [Header("Unique identifier for this item")]
        public string id;
        public abstract Sprite[] getSprites();
        public abstract Sprite getSprite();
        public abstract ItemDisplayType? getDisplayType();
    }
}