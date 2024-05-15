using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Partitions;
using TileMaps.Layer;
using Conduits.Ports;
using Conduits.Systems;
using Items;

namespace Conduits {
    public interface IConduit {
        public int getX();
        public int getY();
        public void setX(int val);
        public void setY(int val);
        public ConduitItem getConduitItem();
        public string getId();
        public void setConduitSystem(IConduitSystem conduitSystem);
        public IConduitSystem getConduitSystem();
    }
}
