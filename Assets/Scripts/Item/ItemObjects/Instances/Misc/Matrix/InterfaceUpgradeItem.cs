using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule {
    public class InterfaceUpgradeItem : ItemObject
    {
        [SerializeField] private Sprite sprite;
        public override Sprite getSprite()
        {
            return sprite;
        }
    }
}

