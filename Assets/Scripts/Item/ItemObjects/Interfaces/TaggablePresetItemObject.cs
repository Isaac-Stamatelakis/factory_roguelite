using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using Items.Tags;

namespace Items {
    [CreateAssetMenu(fileName ="I~New Fluid Container Tile Item",menuName="Item/Tagged Items/Standard")]
    public class TaggablePresetItemObject : PresetItemObject, ITaggableItem
    {
        [SerializeField] private List<ItemTag> tags;
        public List<ItemTag> getTags()
        {
            return tags;
        }
    }
}

