using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles {
    public interface IStateRotationTile
    {
        public TileBase getTile(int rotation, bool mirror);
    }
}

