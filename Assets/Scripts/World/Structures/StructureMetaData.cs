using UnityEngine;

namespace World.Cave.Structures
{
    public enum StructurePlacementRestriction
    {
        None,
        Open,
        Surrounded
    }
    public class StructureMetaData
    {
        public StructurePlacementRestriction PlacementRestriction;
        public StructureMetaData(StructurePlacementRestriction placementRestriction)
        {
            PlacementRestriction = placementRestriction;
        }
    }
}
