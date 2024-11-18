using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Partitions;
using TileMaps.Layer;
using Conduits.Ports;
using Conduits.Systems;
using Items;
using Tiles;

namespace Conduits {
    public interface IConduit {
        public int GetX();
        public int GetY();
        public Vector2Int GetPosition();
        public void SetX(int val);
        public void SetY(int val);
        public ConduitItem GetConduitItem();
        public string GetId();
        public void SetConduitSystem(IConduitSystem newConduitSystem);
        public IConduitSystem GetConduitSystem();
        public int GetActivatedState();
        public int GetState();
        public void SetState(int state);
        public void AddStateDirection(ConduitDirectionState directionState);
        public void RemoveStateDirection(ConduitDirectionState directionState);
        public bool ConnectsDirection(ConduitDirectionState directionState);
    }
}
