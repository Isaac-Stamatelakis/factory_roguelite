using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConduitModule;

namespace ConduitModule.ConduitSystemModule {
    
    public class ItemConduitSystem : AConduitSystem
    {
        
        public ItemConduitSystem(string id) : base(id)
        {
            
        }
        private Dictionary<int, List<ConnectedConduit>> extractions;
        private Dictionary<int, Dictionary<int, List<ConnectedConduit>>> insertions;
        public override void tickUpdate()
        {
            foreach (KeyValuePair<int,List<ConnectedConduit>> kvp in extractions) {
                if (!insertions.ContainsKey(kvp.Key)) {
                    continue;
                }
                Dictionary<int,List<ConnectedConduit>> priorityInsertions = insertions[kvp.Key];
                List<int> priorities = priorityInsertions.Keys.OrderByDescending(k => k).ToList();
                foreach (ConnectedConduit extractionConduit in kvp.Value) {
                    ItemFilter outputFilter = ((ItemConduitOptions) extractionConduit.conduit.GetConduitOptions()).outputFilter;
                    ItemSlot extracted = extractionConduit.conduitPort.extract(outputFilter);
                    if (extracted == null) {
                        continue;
                    }
                    foreach (int priorityValue in priorities) {
                        foreach (ConnectedConduit insertionConduit in priorityInsertions[priorityValue]) {
                            ItemFilter inputFilter = ((ItemConduitOptions) insertionConduit.conduit.GetConduitOptions()).inputFilter;
                            insertionConduit.conduitPort.insert(extracted,inputFilter);
                            if (extracted == null || extracted.amount == 0) {
                                break;
                            }
                        }
                        if (extracted == null || extracted.amount == 0) {
                            break;
                        }
                    }
                }
            }
        }

        protected override void generateSystem()
        {
            extractions = new Dictionary<int, List<ConnectedConduit>>();
            insertions = new Dictionary<int, Dictionary<int, List<ConnectedConduit>>>();
            foreach (IConduit conduit in conduits) {
                
            }
        }

        private class ConnectedConduit {
            public IConduit conduit;
            public IItemConduitPort conduitPort;
            public int distanceFrom(ConnectedConduit connectedConduit) {
                return Mathf.Abs(conduit.getX()-connectedConduit.conduit.getX()) + Mathf.Abs(conduit.getY()-connectedConduit.conduit.getY());
            }
        }
    }
}

