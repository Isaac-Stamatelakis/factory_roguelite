using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc.RandomFrequency;
using Tiles.TileMap.Interval;

namespace WorldModule.Caves {
    public class Structure
    {
        public IntervalVector FirstVariantCoveredArea;
        public List<StructureVariant> variants;

        public Structure(List<StructureVariant> variants, IntervalVector firstVariantCoveredArea)
        {
            this.variants = variants;
            this.FirstVariantCoveredArea = firstVariantCoveredArea;
        }
    }

    [System.Serializable]
    public class StructureVariant {
        [SerializeField] private WorldTileConduitData data;
        [SerializeField] private Vector2Int size;

        public StructureVariant(WorldTileConduitData data, Vector2Int size)
        {
            this.data = data;
            this.size = size;
        }

        public WorldTileConduitData Data { get => data; }
        public Vector2Int Size { get => size;  }
    }

}
