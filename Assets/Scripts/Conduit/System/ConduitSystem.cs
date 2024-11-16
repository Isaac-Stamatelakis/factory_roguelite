using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conduits.Systems {
    public abstract class ConduitSystem<SystemConduit> : IConduitSystem where SystemConduit : IConduit 
        {
        public ConduitSystem(string id) {
            this.id = id;
            Conduits = new HashSet<SystemConduit>();
        }
        protected HashSet<SystemConduit> conduits;
        private string id;
        protected HashSet<SystemConduit> Conduits { get => conduits; set => conduits = value; }

        public virtual void addConduit(IConduit conduit) {
            if (conduit is not SystemConduit systemConduit)  {
                Debug.LogError("Tried to add invalid conduit to system");
                return;
            }
            Conduits.Add(systemConduit);
            conduit.SetConduitSystem(this);
        }

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
            foreach (SystemConduit conduit in otherConduitSystem.getConduits()) {
                addConduit(conduit);
                conduit.SetConduitSystem(this);
            }
        }

        public int getSize()
        {
            return Conduits.Count;
        }

        public bool contains(IConduit conduit)
        {
            if (conduit is not SystemConduit systemConduit) {
                return false;
            }
            return Conduits.Contains(systemConduit);
        }

        public string getId()
        {
            return id;
        }

        public HashSet<IConduit> getConduits()
        {   
            HashSet<IConduit> result = new HashSet<IConduit>();
            foreach (SystemConduit conduit in conduits) {
                result.Add((IConduit) conduit);
            }
            return result;
        }

        public abstract void rebuild();
    }
}

