using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags.Matrix {
    [CreateAssetMenu(fileName ="I~New Encoded Recipe Item",menuName="Item/Tagged Items/Matrix/Drive")]
    public class MatrixDriveItem : PresetItemObject, ITaggableItem
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private int maxItems;
        public int MaxItems {get => maxItems;}
        [SerializeField] private int maxAmount;
        public int MaxAmount {get => maxAmount;}

        public List<ItemTag> getTags()
        {
            return new List<ItemTag>{ItemTag.StorageDrive};
        }
    }
}

