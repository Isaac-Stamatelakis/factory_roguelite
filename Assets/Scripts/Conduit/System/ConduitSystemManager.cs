using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ConduitModule.ConduitSystemModule {

    public interface IConduitSystemManager {

    }
    public class ConduitSystemManager : IConduitSystemManager {
        private List<IConduitSystem> conduitSystems;
        private ConduitType type;

        public ConduitType Type { get => type;}

        public ConduitSystemManager(ConduitType conduitType) {
            conduitSystems = new List<IConduitSystem>();
            this.type = conduitType;
        }

        public void addConduitPartition(IConduit[,] conduits) {
            // Step 1, Generate conduit systems from given conduits
            List<IConduitSystem> newSystems = generateSystemsFromArray(conduits,Global.ChunkPartitionSize,Global.ChunkPartitionSize);
            // Step 2, Merge conduit systems with existing conduit systems
            for (int n = newSystems.Count-1; n >= 0; n--) {
                foreach (IConduitSystem conduitSystem in conduitSystems) {
                    IConduitSystem newSystem = newSystems[n];
                    if (!newSystem.connectsTo(conduitSystem)) {
                        continue;
                    }
                    conduitSystem.merge(newSystem);
                    newSystems.RemoveAt(n);
                    break;
                }
                n--;
            }
            // Remaining new systems are not merged
            conduitSystems.AddRange(newSystems);
        }

        private List<IConduitSystem> generateSystemsFromArray(IConduit[,] conduits, int width, int height) {
            HashSet<IConduit> conduitsNotSeen = new HashSet<IConduit>();
            List<IConduitSystem> conduitSystems = new List<IConduitSystem>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    IConduit conduit = conduits[x,y];
                    if (conduit == null) {
                        continue;
                    }
                    conduitsNotSeen.Add(conduit);
                }
            }
            while (conduitsNotSeen.Count > 0) {
                IConduit conduit = conduitsNotSeen.First();
                conduitsNotSeen.Remove(conduit);
                IConduitSystem conduitSystem = ConduitSystemFactory.create(type);
                conduitSystems.Add(conduitSystem);
                conduitSystem.addConduit(conduit);
                DFSConduit(conduitSystem,conduit,conduits,conduitsNotSeen,width,height); // Search Array for all connecting conduits
            }
            return conduitSystems;
        }

        private void DFSConduit(IConduitSystem conduitSystem, IConduit seenConduit, IConduit[,] conduits,HashSet<IConduit> conduitsNotSeen, int width, int height) {
            int left = seenConduit.getPartitionX()-1;
            if (left >= 0) {
                IConduit leftConduit = conduits[left,seenConduit.getPartitionY()];
                DFSEdge(conduitSystem,leftConduit,conduits,conduitsNotSeen,width,height);
            }
            int right = seenConduit.getPartitionX()+1;
            if (right < width) {
                IConduit rightConduit = conduits[right,seenConduit.getPartitionY()];
                DFSEdge(conduitSystem,rightConduit,conduits,conduitsNotSeen,width,height);
            }
            int down = seenConduit.getPartitionY()-1;
            if (down >= 0) {
                IConduit downConduit = conduits[seenConduit.getPartitionX(),down];
                DFSEdge(conduitSystem,downConduit,conduits,conduitsNotSeen,width,height);
            }
            int up = seenConduit.getPartitionY()+1;
            if (up < height) {
                IConduit upConduit = conduits[seenConduit.getPartitionX(),up];
                DFSEdge(conduitSystem,upConduit,conduits,conduitsNotSeen,width,height);
            }
        }

        private void DFSEdge(IConduitSystem conduitSystem, IConduit newConduit, IConduit[,] conduits, HashSet<IConduit> conduitsNotSeen, int width, int height) {
            if (newConduit == null) { // If null will not be in dict
                return;
            }
            if (!conduitsNotSeen.Contains(newConduit)) { // Already Seen
                return;
            }
            if (conduitSystem.getId() != newConduit.getId()) {
                return;
            }
            conduitSystem.addConduit(newConduit);
            conduitsNotSeen.Remove(newConduit);
            DFSConduit(conduitSystem,newConduit,conduits,conduitsNotSeen,width,height);
        }
        public IConduit[,] removePartition() {
            return null;
        }

        public void mergeConduitSystems(IConduitSystem conduitSystemA, IConduitSystem conduitSystemB) {

        }

    }
}