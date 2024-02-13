using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;

namespace ConduitModule.ConduitSystemModule {
    public interface IConduitSystem {
        public void tickUpdate();
    }

    public class ConduitSystem : IConduitSystem {
        public ConduitSystem() {
            Conduits = new HashSet<Conduit>();
            colorListExtractionsDict = new Dictionary<int, TileEntity>();
            priorityDictColorListInsertitionsDict = new Dictionary<int, Dictionary<int, List<TileEntity>>>();
        }
        protected Dictionary<int, TileEntity> colorListExtractionsDict;
        protected Dictionary<int, Dictionary<int,List<TileEntity>>> priorityDictColorListInsertitionsDict;
        private HashSet<Conduit> conduits;

        public HashSet<Conduit> Conduits { get => conduits; set => conduits = value; }

        public void tickUpdate()
        {
            throw new System.NotImplementedException();
        }
        public void addConduit(Conduit conduit) {
            Conduits.Add(conduit);
        }
        public bool connectsTo(ConduitSystem otherConduitSystem) {
            // If otherConduitSystem is smaller, check it instead
            if (otherConduitSystem.Conduits.Count < conduits.Count) {
                return otherConduitSystem.connectsTo(this);
            }
            // Check otherConduitSystem contains any conduit in this one. O(n)
            foreach (Conduit conduit in Conduits) {
                if (otherConduitSystem.Conduits.Contains(conduit)) {
                    return true;
                }
            }
            return false;
        }
        public void merge(ConduitSystem otherConduitSystem) {
            
        }
    }
}