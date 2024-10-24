using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits.Ports;
using TileEntityModule;

namespace Conduits.Systems {

    public interface IConduitSystemManager {
        public void setConduit(int x, int y, IConduit conduit);
        public ITileEntityInstance getTileEntityAtPosition(int x, int y);
        public ConduitType getConduitType();
        public Dictionary<ITileEntityInstance, List<TileEntityPort>> getTileEntityPorts();
        public EntityPortType getPortTypeAtPosition(int x, int y);
        public void addTileEntity(ITileEntityInstance tileEntity);
        public void deleteTileEntity(Vector2Int position);
    }

    public interface ITickableConduitSystem {
        public void tickUpdate();
    }
    public abstract class ConduitSystemManager<TConduit, TSystem> : IConduitSystemManager where TConduit : IConduit where TSystem : IConduitSystem {
        protected List<TSystem> conduitSystems;
        protected ConduitType type;
        protected TConduit[,] conduits;
        protected Dictionary<ITileEntityInstance, List<TileEntityPort>> chunkConduitPorts;
        protected Vector2Int size;
        protected Vector2Int referencePosition;

        public ConduitType Type { get => type;}
        public Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityConduitPorts { get => chunkConduitPorts; set => chunkConduitPorts = value; }

        public ConduitSystemManager(ConduitType conduitType, TConduit[,] conduits, Vector2Int size,Dictionary<ITileEntityInstance, List<TileEntityPort>> chunkConduitPorts, Vector2Int referencePosition) {
            this.type = conduitType;
            this.conduits = conduits;
            this.size = size;
            this.tileEntityConduitPorts = chunkConduitPorts;
            this.referencePosition = referencePosition;
            conduitSystems = new List<TSystem>();
            generateSystemsFromArray();
        }

        public TConduit getConduitCellPosition(Vector2Int position) {
            Vector2Int relativePosition = position-referencePosition;
            if (!inBounds(relativePosition)) {
                return default(TConduit);
            }
            return conduits[relativePosition.x,relativePosition.y];
        }
        public void addTileEntity(ITileEntityInstance tileEntity) {
            if (tileEntity is not IConduitInteractable interactable) {
                return;
            }
            ConduitPortLayout layout = interactable.getConduitPortLayout();
            if (layout == null) {
                return;
            }
            switch (type) {
                case ConduitType.Item:
                    chunkConduitPorts[tileEntity] = layout.itemPorts;
                    break;
                case ConduitType.Fluid:
                    chunkConduitPorts[tileEntity] = layout.fluidPorts;
                    break;
                case ConduitType.Energy:
                    chunkConduitPorts[tileEntity] = layout.energyPorts;
                    break;
                case ConduitType.Signal:
                    chunkConduitPorts[tileEntity] = layout.signalPorts;
                    break;
                case ConduitType.Matrix:
                    chunkConduitPorts[tileEntity] = layout.matrixPorts;
                    break;
            }
            foreach (TileEntityPort port in chunkConduitPorts[tileEntity]) {
                Vector2Int position = port.position + tileEntity.getCellPosition() - referencePosition;
                if (!inBounds(position)) {
                    continue;
                }
                TConduit conduit = conduits[position.x,position.y];
                if (conduit == null) {
                    continue;
                }
                onTileEntityAdd(conduit,tileEntity,port);
            }
        }

        public abstract void onTileEntityAdd(TConduit conduit,ITileEntityInstance tileEntity, TileEntityPort port);
        public abstract void onTileEntityRemoved(TConduit conduit);
        public void deleteTileEntity(Vector2Int position) {
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPort>> kvp in chunkConduitPorts) {
                if (kvp.Key.getCellPosition() == position) {
                    foreach (TileEntityPort port in chunkConduitPorts[kvp.Key]) {
                        Vector2Int portPosition = port.position + position-referencePosition;
                        if (!inBounds(portPosition)) {
                            continue;
                        }
                        TConduit conduit = conduits[portPosition.x,portPosition.y];
                        if (conduit == null) {
                            continue;
                        }
                        onTileEntityRemoved(conduit);
                    }
                    tileEntityConduitPorts.Remove(kvp.Key);
                    return;
                }
            }
        }

        protected bool inBounds(Vector2Int position) {
            return position.x >= 0 && position.x < size.x && position.y >= 0 && position.y < size.y;
        }
        public void setConduit(int x, int y, IConduit conduit) {
            
            x -= referencePosition.x;
            y -= referencePosition.y;
            if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
                Debug.LogWarning("Conduit Manager for " + type.ToString() + " tried to set conduit out of bounds [" + x + "," + y +"]");
                return;
            }
            if (conduit == null) {
                if (conduits[x,y] != null) {
                    removeConduitFromSystem(conduits[x,y],x,y);
                }
                return;
            }
            if (conduit is not TConduit typeConduit) {
                Debug.LogError("Tried to add invalid conduit type to conduit system");
                return;
            }
            conduit.setX(x);
            conduit.setY(y);
            conduits[x,y] = typeConduit;
            updateSystemsOnPlace(conduit,x,y);
        }

        private void updateSystemsOnPlace(IConduit conduit, int x, int y) {
            List<TSystem> systemsToMerge = new List<TSystem>();
            if (x + 1 < size.x) {
                IConduit right = conduits[x+1,y];
                if (right != null) {
                    systemsToMerge.Add((TSystem)right.getConduitSystem());
                }
            }
            if (x - 1 >= 0) {
                IConduit left = conduits[x-1,y];
                if (left != null) {
                    systemsToMerge.Add((TSystem)left.getConduitSystem());
                }
            }
            if (y + 1 < size.y) {
                IConduit up = conduits[x,y+1];
                if (up != null) {
                    systemsToMerge.Add((TSystem)up.getConduitSystem());
                }
            }
            if (y - 1 >= 0) {
                IConduit down = conduits[x,y-1];
                if (down != null) {
                    systemsToMerge.Add((TSystem)down.getConduitSystem());
                }
            }
            IConduitSystem newSystem = ConduitSystemFactory.create(conduit);
            if (newSystem is not TSystem system) {
                Debug.LogError("Tried to add invalid system to conduit manager");
                return;
            }
            newSystem.addConduit(conduit);
            if (systemsToMerge.Count == 0) { // No systems to merge, create new system
                conduitSystems.Add(system);
                return;
            }
            TSystem mergeInto = systemsToMerge[0];
            mergeInto.merge(newSystem);
            for (int i = 1; i < systemsToMerge.Count; i++) {
                TSystem toMerge = systemsToMerge[i];
                if (toMerge == null || toMerge.Equals(mergeInto)) {
                    continue;
                }
                mergeInto.merge(toMerge);
                conduitSystems.Remove(toMerge);
            }
            mergeInto.rebuild();
        }

        private void removeConduitFromSystem(IConduit conduit, int x, int y) {
            // Step 1, delete conduit system
            TSystem conduitSystem = (TSystem) conduit.getConduitSystem();
            if (conduitSystem == null) {
                Debug.LogError("Conduit somehow didn't belong to a conduit system");
            }
            conduitSystems.Remove(conduitSystem);
            // Step 2, Run DFS on conduit to remove setting the conduit system of all connecting conduits to null
            DFSNull(conduit,conduit.getConduitItem().id);

            // Step 3, delete the conduit from the conduit array
            conduits[x,y] = default(TConduit);

            // Step 4, Regenerate systems by running DFSConduit on up, left, down, right
            if (x + 1 < size.x) {
                IConduit right = conduits[x+1,y];
                if (right != null && right.getConduitSystem() == null) {
                    TSystem system = (TSystem)ConduitSystemFactory.create(right);
                    conduitSystems.Add(system);
                    DFSConduit(right,system);
                }
                
            }
            if (x - 1 >= 0) {
                IConduit left = conduits[x-1,y];
                if (left != null && left.getConduitSystem() == null) {
                    TSystem system = (TSystem)ConduitSystemFactory.create(left);
                    conduitSystems.Add(system);
                    DFSConduit(left,system);
                }
            }
            if (y + 1 < size.y) {
                IConduit up = conduits[x,y+1];
                if (up != null && up.getConduitSystem() == null) {
                    TSystem system = (TSystem)ConduitSystemFactory.create(up);
                    conduitSystems.Add(system);
                    DFSConduit(up,system);
                }
            }
            if (y - 1 >= 0) {
                IConduit down = conduits[x,y-1];
                if (down != null && down.getConduitSystem() == null) {
                    TSystem system = (TSystem)ConduitSystemFactory.create(down);
                    conduitSystems.Add(system);
                    DFSConduit(down,system);
                }
            }
        }

        public IConduit[,] getConduitPartitionData(Vector2Int partitionPosition) {
            IConduit[,] partitionConduits = new IConduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            Vector2Int position = partitionPosition*Global.ChunkPartitionSize-referencePosition;
            if (position.x < 0 || position.x > size.x || position.y < 0 || position.y > size.y) {
                Debug.LogError("Attempted to get partition conduit data out of range");
                return null;
            }
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    partitionConduits[x,y] = conduits[position.x+x,position.y+y];
                }
            }
            return partitionConduits;
        }

        public ITileEntityInstance getTileEntityAtPosition(int x, int y) {
            // TODO make this more efficent
            Vector2Int pos = new Vector2Int(x,y);
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPort>> kvp in chunkConduitPorts) {
                //Vector2Int distVec = kvp.Key - new Vector2Int(x,y);
                foreach (TileEntityPort tileEntityPort in kvp.Value) {
                    // Key is tileEntityPosition
                    if (tileEntityPort.position + kvp.Key.getCellPosition() == pos) { // Conduit is placed on tileEntityPort
                        return kvp.Key;
                    }
                }
            }
            return null;
        }
        public EntityPortType getPortTypeAtPosition(int x, int y) {
            // TODO make this more efficent
            Vector2Int pos = new Vector2Int(x,y);
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPort>> kvp in chunkConduitPorts) {
                //Vector2Int distVec = kvp.Key - new Vector2Int(x,y);
                foreach (TileEntityPort tileEntityPort in kvp.Value) {
                    // Key is tileEntityPosition
                    if (tileEntityPort.position + kvp.Key.getCellPosition() == pos) { // Conduit is placed on tileEntityPort
                        return tileEntityPort.portType;
                    }
                }
            }
            return EntityPortType.None;
        }
        /// <summary>
        /// Sets the conduit system of each conduit seen to null
        /// </summary>
        private void DFSNull(IConduit conduit,string id) {
            if (conduit == null || conduit.getConduitSystem() == null || conduit.getConduitItem().id != id) {
                return;
            }
            conduit.setConduitSystem(null);
            int left = conduit.getX()-1;
            if (left >= 0) {
                IConduit leftConduit = conduits[left,conduit.getY()];
                DFSNull(leftConduit,id);
            }
            int right = conduit.getX()+1;
            if (right < size.x) {
                IConduit rightConduit = conduits[right,conduit.getY()];
                DFSNull(rightConduit,id);
            }
            int down = conduit.getY()-1;
            if (down >= 0) {
                IConduit downConduit = conduits[conduit.getX(),down];
                DFSNull(downConduit,id);
            }
            int up = conduit.getY()+1;
            if (up < size.y) {
                IConduit upConduit = conduits[conduit.getX(),up];
                DFSNull(upConduit,id);
            }
        }
        private void generateSystemsFromArray() {
            HashSet<IConduit> conduitsNotSeen = new HashSet<IConduit>();
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    IConduit conduit = conduits[x,y];
                    if (conduit == null || conduit.getConduitItem() == null) {
                        continue;
                    }
                    conduitsNotSeen.Add(conduit);
                }
            }
            while (conduitsNotSeen.Count > 0) {
                IConduit conduit = conduitsNotSeen.First();
                conduitsNotSeen.Remove(conduit);
                if (conduit.getConduitSystem() != null) {
                    continue;
                }
                TSystem conduitSystem = (TSystem)ConduitSystemFactory.create(conduit);
                conduitSystems.Add(conduitSystem);
                DFSConduit(conduit, conduitSystem); // Search Array for all connecting conduits
            }
            onGenerationCompleted();
        }

        public abstract void onGenerationCompleted();

        private void DFSConduit(IConduit conduit,IConduitSystem conduitSystem) {
            if (conduit == null) {
                return;
            }
            if (conduit.getConduitSystem() != null) {
                return;
            }
            if (conduit.getConduitItem().id != conduitSystem.getId()) {
                return;
            }
            conduitSystem.addConduit(conduit);
            int left = conduit.getX()-1;
            if (left >= 0) {
                IConduit leftConduit = conduits[left,conduit.getY()];
                DFSConduit(leftConduit,conduitSystem);
            }
            int right = conduit.getX()+1;
            if (right < size.x) {
                IConduit rightConduit = conduits[right,conduit.getY()];
                DFSConduit(rightConduit,conduitSystem);
            }
            int down = conduit.getY()-1;
            if (down >= 0) {
                IConduit downConduit = conduits[conduit.getX(),down];
                DFSConduit(downConduit,conduitSystem);
            }
            int up = conduit.getY()+1;
            if (up < size.y) {
                IConduit upConduit = conduits[conduit.getX(),up];
                DFSConduit(upConduit,conduitSystem);
            }
        }

        public ConduitType getConduitType()
        {
            return type;
        }

        public Dictionary<ITileEntityInstance, List<TileEntityPort>> getTileEntityPorts()
        {
            return tileEntityConduitPorts;
        }
    }
}