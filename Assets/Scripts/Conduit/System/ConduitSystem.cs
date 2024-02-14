using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule.PartitionModule;

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

    public interface IConduitPortExtractions<Port> where Port : IConduitPort {

    }

    public interface IConduitPortInsertions<Port> where Port : IConduitPort {

    }

    public abstract class AConduitSystem : IConduitSystem  {
        public AConduitSystem(string id) {
            this.id = id;
            conduits = new HashSet<IConduit>();
            //colorListExtractionsDict = new Dictionary<int, Port>();
            //priorityDictColorListInsertitionsDict = new Dictionary<int, Dictionary<int, List<Port>>>();
        }
        protected HashSet<IConduit> conduits;
        private string id;
        protected HashSet<Vector2Int> conduitPositions;

        public abstract void tickUpdate();
        public void addConduit(IConduit conduit) {
            conduits.Add(conduit);
        }
        public bool containsPosition(Vector2Int vector2Int) {
            if (conduitPositions == null) {
                return false;
            }
            return this.conduitPositions.Contains(vector2Int);
        }

        public void generate() {
            generatePositions();
            generateSystem();
        }

        private void generatePositions() {
            conduitPositions = new HashSet<Vector2Int>();
            foreach (IConduit conduit in conduits) {
                conduitPositions.Add(new Vector2Int(conduit.getX(),conduit.getY()));
            }
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