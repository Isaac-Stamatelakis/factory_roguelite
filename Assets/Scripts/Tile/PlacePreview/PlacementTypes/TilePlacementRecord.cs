using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps.Previewer
{
    public abstract class TilePlacementRecord
    {
        protected Tilemap tilemap;
        private bool cleared;
        protected TilePlacementRecord(string id, Tilemap tilemap)
        {
            Id = id;
            this.tilemap = tilemap;
        }
        public string Id { get; }

        public void Clear()
        {
            if (cleared) return;
            cleared = true;
            DoClear();
        }
        protected abstract void DoClear();
        public abstract bool RecordMatch(Vector3Int position, string id);
    }

    public class SingleTilePlacementRecord : TilePlacementRecord
    {
        private readonly Vector3Int placePosition;
        public SingleTilePlacementRecord(string id, Vector3Int placePosition, Tilemap tilemap) : base(id,tilemap)
        {
            this.placePosition = placePosition;
        }

        protected override void DoClear()
        {
            tilemap.SetTile(placePosition, null);
        }

        public override bool RecordMatch(Vector3Int position, string id)
        {
            return position == placePosition && id == Id;
        }
    }

    public class MultiTilePlacementRecord : TilePlacementRecord
    {
        private readonly List<Vector3Int> placePositions;
        public MultiTilePlacementRecord(string id, List<Vector3Int> placePositions, Tilemap tilemap) : base(id, tilemap)
        {
            this.placePositions = placePositions;
        }

        protected override void DoClear()
        {
            foreach (Vector3Int position in placePositions)
            {
                tilemap.SetTile(position, null);
            }
        }

        public override bool RecordMatch(Vector3Int position, string id)
        {
            const int originPosition = 0;
            return position == placePositions[originPosition] && id == Id;
        }
    }

    public class MultiMapPlacementRecord : TilePlacementRecord
    {
        private readonly Vector3Int primaryPosition;
        private readonly List<Vector3Int> secondaryPositions;
        private Tilemap unhighlightedMap;
        public MultiMapPlacementRecord(string id, Tilemap tilemap, Tilemap unhighlightedMap, Vector3Int primaryPosition, List<Vector3Int> secondaryPositions) : base(id, tilemap)
        {
            this.unhighlightedMap = unhighlightedMap;
            this.primaryPosition = primaryPosition;
            this.secondaryPositions = secondaryPositions;
        }

        protected override void DoClear()
        {
            tilemap.SetTile(primaryPosition, null);
            foreach (Vector3Int position in secondaryPositions)
            {
                unhighlightedMap.SetTile(position, null);
            }
        }

        public override bool RecordMatch(Vector3Int position, string id)
        {
            return position == primaryPosition && id == Id;
        }
    }
}