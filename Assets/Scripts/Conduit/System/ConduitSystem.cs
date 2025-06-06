using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conduits.Systems {
    public abstract class ConduitSystem<TSystemConduit> : IConduitSystem where TSystemConduit : IConduit
    {
        protected ConduitSystem(string id, IConduitSystemManager manager)
        {
            this.id = id;
            this.manager = manager;
            conduits = new HashSet<TSystemConduit>();
        }

        protected HashSet<TSystemConduit> conduits;
        protected IConduitSystemManager manager;
        private string id;

        protected HashSet<TSystemConduit> Conduits
        {
            get => conduits;
        }

        public virtual void AddConduit(IConduit conduit)
        {
            if (conduit is not TSystemConduit systemConduit)
            {
                Debug.LogError("Tried to add invalid conduit to system");
                return;
            }

            Conduits.Add(systemConduit);
            conduit.SetConduitSystem(this);
        }

        public bool ConnectsTo(IConduitSystem otherConduitSystem)
        {
            // If otherConduitSystem is smaller, check it instead
            if (otherConduitSystem.GetSize() < Conduits.Count)
            {
                return otherConduitSystem.ConnectsTo(this);
            }

            // Check otherConduitSystem contains any conduit in this one. O(n)
            foreach (TSystemConduit conduit in Conduits)
            {
                if (otherConduitSystem.Contains(conduit))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void Merge(IConduitSystem otherConduitSystem)
        {
            foreach (TSystemConduit conduit in otherConduitSystem.GetConduits())
            {
                AddConduit(conduit);
                conduit.SetConduitSystem(this);
            }
        }

        public int GetSize()
        {
            return Conduits.Count;
        }

        public bool Contains(IConduit conduit)
        {
            if (conduit is not TSystemConduit systemConduit)
            {
                return false;
            }

            return Conduits.Contains(systemConduit);
        }

        public string GetId()
        {
            return id;
        }

        public HashSet<IConduit> GetConduits()
        {
            HashSet<IConduit> result = new HashSet<IConduit>();
            foreach (TSystemConduit conduit in conduits)
            {
                result.Add((IConduit)conduit);
            }

            return result;
        }

        public abstract void Rebuild();
        public abstract bool IsActive();

        public void ClearConduits()
        {
            conduits.Clear();
        }
    }
}

