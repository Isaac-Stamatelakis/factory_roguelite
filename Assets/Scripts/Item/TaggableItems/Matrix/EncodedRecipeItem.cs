using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Tags.Matrix {
    [CreateAssetMenu(fileName ="I~New Encoded Recipe Item",menuName="Item/Tagged Items/Matrix/Recipe")]
    public class EncodedRecipeItem : ItemObject, ITaggable
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Sprite encodedSprite;
        public override Sprite getSprite()
        {
            return sprite;
        }
    }
}

