using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule {
    public interface ITaggable
    {

    }

    public interface ICloneableTag {
        public object copy();
    }
}

