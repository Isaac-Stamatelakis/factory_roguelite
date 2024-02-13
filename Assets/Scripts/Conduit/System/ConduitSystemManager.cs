using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ConduitModule.ConduitSystemModule {
    public class ConduitSystemManager {
        private List<ConduitSystem> conduitSystems;
        public ConduitSystemManager() {
            conduitSystems = new List<ConduitSystem>();
        }

        public void addConduitPartition(Conduit[,] conduits) {
            // Step 1, Generate conduit systems from given conduits
            List<ConduitSystem> newSystems = generateSystemsFromArray(conduits,Global.ChunkPartitionSize,Global.ChunkPartitionSize);
            // Step 2, Merge conduit systems with existing conduit systems
            for (int n = newSystems.Count-1; n >= 0; n--) {
                foreach (ConduitSystem conduitSystem in conduitSystems) {
                    ConduitSystem newSystem = newSystems[n];
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

        private List<ConduitSystem> generateSystemsFromArray(Conduit[,] conduits, int width, int height) {
            HashSet<Conduit> conduitsNotSeen = new HashSet<Conduit>();
            List<ConduitSystem> conduitSystems = new List<ConduitSystem>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Conduit conduit = conduits[x,y];
                    if (conduit == null) {
                        continue;
                    }
                    conduitsNotSeen.Add(conduit);
                }
            }
            while (conduitsNotSeen.Count > 0) {
                Conduit conduit = conduitsNotSeen.First();
                conduitsNotSeen.Remove(conduit);
                ConduitSystem conduitSystem = new ConduitSystem();
                conduitSystems.Add(conduitSystem);
                conduitSystem.addConduit(conduit);
                DFSConduit(conduitSystem,conduit,conduits,conduitsNotSeen,width,height); // Search Array for all connecting conduits
            }
            return conduitSystems;
        }

        private void DFSConduit(ConduitSystem conduitSystem, Conduit seenConduit, Conduit[,] conduits,HashSet<Conduit> conduitsNotSeen, int width, int height) {
            int left = seenConduit.PartitionX-1;
            if (left >= 0) {
                Conduit leftConduit = conduits[left,seenConduit.PartitionY];
                DFSEdge(conduitSystem,leftConduit,conduits,conduitsNotSeen,width,height);
            }
            int right = seenConduit.PartitionX+1;
            if (right < width) {
                Conduit rightConduit = conduits[right,seenConduit.PartitionY];
                DFSEdge(conduitSystem,rightConduit,conduits,conduitsNotSeen,width,height);
            }
            int down = seenConduit.PartitionY-1;
            if (down >= 0) {
                Conduit downConduit = conduits[seenConduit.PartitionX,down];
                DFSEdge(conduitSystem,downConduit,conduits,conduitsNotSeen,width,height);
            }
            int up = seenConduit.PartitionY+1;
            if (up < height) {
                Conduit upConduit = conduits[seenConduit.PartitionX,up];
                DFSEdge(conduitSystem,upConduit,conduits,conduitsNotSeen,width,height);
            }
        }

        private void DFSEdge(ConduitSystem conduitSystem, Conduit newConduit, Conduit[,] conduits, HashSet<Conduit> conduitsNotSeen, int width, int height) {
            if (newConduit == null) { // If null will not be in dict
                return;
            }
            if (!conduitsNotSeen.Contains(newConduit)) { // Already Seen
                return;
            }
            conduitSystem.addConduit(newConduit);
            conduitsNotSeen.Remove(newConduit);
            DFSConduit(conduitSystem,newConduit,conduits,conduitsNotSeen,width,height);
        }
        public Conduit[,] removePartition() {
            return null;
        }

        public void mergeConduitSystems(ConduitSystem conduitSystemA, ConduitSystem conduitSystemB) {

        }

    }
}