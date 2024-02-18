using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule.PartitionModule;
using ConduitModule.Ports;
using System.Linq;

namespace ConduitModule.ConduitSystemModule {
    public interface IConduitSystem {
        public void tickUpdate();
        public int getSize();
        public bool connectsTo(IConduitSystem conduitSystem);
        public bool contains(IConduit conduit);
        public void merge(IConduitSystem conduitSystem);
        public void addConduit(IConduit conduit);
        public string getId();
        public HashSet<IConduit> getConduits();
        public void rebuild();
    }
    public abstract class ConduitSystem<Port> : IConduitSystem where Port : IConduitPort{
        public ConduitSystem(string id) {
            this.id = id;
            Conduits = new HashSet<IConduit>();
            init();
        }
        protected HashSet<IConduit> conduits;
        private string id;

        protected abstract void init();
        protected HashSet<IConduit> Conduits { get => conduits; set => conduits = value; }

        public abstract void tickUpdate();
        public void addConduit(IConduit conduit) {
            Conduits.Add(conduit);
            conduit.setConduitSystem(this);
            addConduitToStructures(conduit);
        }

        public abstract void addConduitToStructures(IConduit conduit);

        public bool connectsTo(IConduitSystem otherConduitSystem) {
            // If otherConduitSystem is smaller, check it instead
            if (otherConduitSystem.getSize() < Conduits.Count) {
                return otherConduitSystem.connectsTo(this);
            }
            // Check otherConduitSystem contains any conduit in this one. O(n)
            foreach (IConduit conduit in Conduits) {
                if (otherConduitSystem.contains(conduit)) {
                    return true;
                }
            }
            return false;
        }
        public virtual void merge(IConduitSystem otherConduitSystem) {
            foreach (IConduit conduit in otherConduitSystem.getConduits()) {
                addConduit(conduit);
                conduit.setConduitSystem(this);
            }
        }

        public int getSize()
        {
            return Conduits.Count;
        }

        public bool contains(IConduit conduit)
        {
            return Conduits.Contains(conduit);
        }

        public string getId()
        {
            return id;
        }

        public HashSet<IConduit> getConduits()
        {
            return conduits;
        }

        public abstract void rebuild();
    }
}