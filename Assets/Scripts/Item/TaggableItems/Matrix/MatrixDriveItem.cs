using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Tags.Matrix {
    [CreateAssetMenu(fileName ="I~New Encoded Recipe Item",menuName="Item/Tagged Items/Matrix/Drive")]
    public class MatrixDriveItem : ItemObject, ITaggable
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private int maxItems;
        public int MaxItems {get => maxItems;}
        [SerializeField] private int maxAmount;
        public int MaxAmount {get => maxAmount;}
        public override Sprite getSprite()
        {
            return sprite;
        }
    }
}

