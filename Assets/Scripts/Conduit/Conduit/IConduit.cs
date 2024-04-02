using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.PartitionModule;
using TileMapModule.Layer;
using ConduitModule.Ports;
using ConduitModule.Systems;

namespace ConduitModule {
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
