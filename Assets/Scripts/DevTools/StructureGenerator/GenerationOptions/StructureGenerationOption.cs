using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevTools.Structures {
    public abstract class StructureGenerationOption
    {

        protected StructureGenerationOption()
        {
     
        }

        public abstract void apply(WorldTileConduitData worldData);
    }
}
