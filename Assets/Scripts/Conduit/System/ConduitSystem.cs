using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule.PartitionModule;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {
    public interface IConduitSystem {
        public void tickUpdate();
        public int getSize();
        public bool connectsTo(IConduitSystem conduitSystem);
        public bool contains(IConduit conduit);
        public void merge(IConduitSystem conduitSystem);
        public void addConduit(IConduit conduit);
        public string getId();
    }
    public abstract class ConduitSystem<Port> : IConduitSystem {
        public ConduitSystem(string id) {
            this.id = id;
            conduits = new HashSet<IConduit>();
        }
        protected HashSet<IConduit> conduits;
        protected List<Port> ports;
        private string id;

        public abstract void tickUpdate();
        public void addConduit(IConduit conduit) {
            conduits.Add(conduit);
        }

        public void generate() {
            generateSystem();
        }

        

        protected abstract void generateSystem();
        public bool connectsTo(IConduitSystem otherConduitSystem) {
            // If otherConduitSystem is smaller, check it instead
            if (otherConduitSystem.getSize() < conduits.Count) {
                return otherConduitSystem.connectsTo(this);
            }
            // Check otherConduitSystem contains any conduit in this one. O(n)
            foreach (IConduit conduit in conduits) {
                if (otherConduitSystem.contains(conduit)) {
                    return true;
                }
            }
            return false;
        }
        public void merge(IConduitSystem otherConduitSystem) {

        }

        public int getSize()
        {
            return conduits.Count;
        }

        public bool contains(IConduit conduit)
        {
            return conduits.Contains(conduit);
        }

        public string getId()
        {
            return id;
        }
    }
}