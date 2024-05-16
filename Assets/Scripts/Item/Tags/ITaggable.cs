using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Tags {
    public interface ITaggableItem
    {
        public List<ItemTag> getTags();
    }

    public interface ICloneableTag {
        public object copy();
    }
}

