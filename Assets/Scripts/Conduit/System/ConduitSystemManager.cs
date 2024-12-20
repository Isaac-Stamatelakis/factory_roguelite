using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits.Ports;
using TileEntityModule;
using System;
using TileMaps.Conduit;
using Tiles;

namespace Conduits.Systems {
    public enum ConduitPlacementMode
    {
        Any,
        New
    }

    public interface IConduitSystemManager {
        public void SetConduit(int x, int y, IConduit conduit);
        public ITileEntityInstance GetTileEntityAtPosition(int x, int y);
        public ConduitType GetConduitType();
        public Dictionary<ITileEntityInstance, List<TileEntityPort>> GetTileEntityPorts();
        public EntityPortType GetPortTypeAtPosition(int x, int y);
        public void AddTileEntity(ITileEntityInstance tileEntity);
        public void DeleteTileEntity(Vector2Int position);
        public IConduit GetIConduitAtCellPosition(Vector2Int position);
        public IConduit GetIConduitAtRelativeCellPosition(Vector2Int position);
        public int GetNewState(Vector2Int position, ConduitPlacementMode placementMode, string id);
        public void RefreshSystemTiles(IConduitSystem conduitSystem);
        public void RefreshConduitTile(IConduit conduit);
        public void SetTileMap(ConduitTileMap conduitTileMap);
        public IConduit[,] GetConduitPartitionData(Vector2Int partitionPosition);
        public Vector2Int GetOffset();
    }

    public interface ITickableConduitSystem {
        public void tickUpdate();
    }
    public abstract class ConduitSystemManager<TConduit, TSystem> : IConduitSystemManager where TConduit : IConduit where TSystem : IConduitSystem {
        protected List<TSystem> conduitSystems;
        protected ConduitType type;
        protected Dictionary<Vector2Int, TConduit> conduits;
        protected Dictionary<ITileEntityInstance, List<TileEntityPort>> chunkConduitPorts;
        protected Vector2Int size;
        protected Vector2Int referencePosition;
        protected ConduitTileMap conduitTileMap;

        public ConduitType Type { get => type;}
        public Dictionary<ITileEntityInstance, List<TileEntityPort>> tileEntityConduitPorts { get => chunkConduitPorts; set => chunkConduitPorts = value; }

        protected ConduitSystemManager(
            ConduitType conduitType, 
            Dictionary<Vector2Int, TConduit> conduits, 
            Vector2Int size,
            Dictionary<ITileEntityInstance, List<TileEntityPort>> chunkConduitPorts, 
            Vector2Int referencePosition
            ) {
            this.type = conduitType;
            this.conduits = conduits;
            this.size = size;
            this.tileEntityConduitPorts = chunkConduitPorts;
            this.referencePosition = referencePosition;
            conduitSystems = new List<TSystem>();
            generateSystemsFromArray();
        }

        public TConduit GetConduitAtRelativeCellPosition(Vector2Int position) {
            Vector2Int relativePosition = position-referencePosition;
            return conduits.TryGetValue(relativePosition, out var cellConduit) ? cellConduit : default(TConduit);
        }
        
        public TConduit GetConduitAtCellPosition(Vector2Int position) {
            return conduits.TryGetValue(position, out var cellConduit) ? cellConduit : default(TConduit);
        }

        public IConduit GetIConduitAtCellPosition(Vector2Int position)
        {
            return GetConduitAtCellPosition(position);
        }
        
        public void AddTileEntity(ITileEntityInstance tileEntity) {
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
                if (!conduits.TryGetValue(position, out var conduit))
                {
                    continue;
                }
                if (conduit == null) {
                    continue;
                }
                onTileEntityAdd(conduit,tileEntity,port);
            }
        }

        public abstract void onTileEntityAdd(TConduit conduit,ITileEntityInstance tileEntity, TileEntityPort port);
        public abstract void onTileEntityRemoved(TConduit conduit);
        public void DeleteTileEntity(Vector2Int position) {
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPort>> kvp in chunkConduitPorts) {
                if (kvp.Key.getCellPosition() == position) {
                    foreach (TileEntityPort port in chunkConduitPorts[kvp.Key]) {
                        Vector2Int portPosition = port.position + position-referencePosition;
                        TConduit conduit = GetConduitAtCellPosition(portPosition);
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

        public IConduit GetIConduitAtRelativeCellPosition(Vector2Int position)
        {
            return GetConduitAtRelativeCellPosition(position);
        }

        /// <summary>
        /// Returns the specific connection state that a conduit placed at position should be with given mode
        /// </summary>
        /// <param name="position">Position of new conduit in world cell space</param>
        /// <param name="placementMode"></param>
        /// <param name="id"></param>
        /// <returns>State of new conduit</returns>
        public int GetNewState(Vector2Int position, ConduitPlacementMode placementMode, string id)
        {
            Vector2Int relativePosition = position-referencePosition;
            IConduit left = GetConduitAtCellPosition(relativePosition+Vector2Int.left);
            int state = 0;
            if (left != null && left.GetId() == id)
            {
                state += (int)ConduitDirectionState.Left;
            }
            IConduit right = GetConduitAtCellPosition(relativePosition+Vector2Int.right);
            if (right != null && right.GetId() == id)
            {
                state += (int)ConduitDirectionState.Right;
            }
            IConduit up = GetConduitAtCellPosition(relativePosition+Vector2Int.up);
            if (up != null && up.GetId() == id)
            {
                state += (int)ConduitDirectionState.Up;
            }
            IConduit down = GetConduitAtCellPosition(relativePosition+Vector2Int.down);
            if (down != null && down.GetId() == id)
            {
                state += (int)ConduitDirectionState.Down;
            }

            return state;
        }

        public void RefreshSystemTiles(IConduitSystem conduitSystem)
        {
            bool noTileMapLoaded = !conduitTileMap;
            if (noTileMapLoaded)
            {
                return;
            }
            foreach (var conduit in conduitSystem.GetConduits())
            {
                RefreshConduitTile(conduit);
            }
        }

        public void RefreshConduitTile(IConduit conduit)
        {
            bool noTileMapLoaded = !conduitTileMap;
            if (noTileMapLoaded)
            {
                return;
            }
            conduitTileMap.RefreshTile(conduit.GetX()+referencePosition.x,conduit.GetY()+referencePosition.y);
        }

        public void SetTileMap(ConduitTileMap conduitTileMap)
        {
            this.conduitTileMap = conduitTileMap;
        }

        public Vector2Int GetOffset()
        {
            return referencePosition;
        }

        public void SetConduit(int x, int y, IConduit conduit) {
            
            x -= referencePosition.x;
            y -= referencePosition.y;
            if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
                Debug.LogWarning("Conduit Manager for " + type.ToString() + " tried to set conduit out of bounds [" + x + "," + y +"]");
                return;
            }
            if (conduit == null) {
                var currentConduit = GetConduitAtCellPosition(new Vector2Int(x, y));
                if (currentConduit != null) {
                    removeConduitFromSystem(currentConduit,x,y);
                }
                return;
            }
            if (conduit is not TConduit typeConduit) {
                Debug.LogError("Tried to add invalid conduit type to conduit system");
                return;
            }
            conduit.SetX(x);
            conduit.SetY(y);
            conduits[new Vector2Int(x, y)] = typeConduit;
            updateSystemsOnPlace(conduit,x,y);
        }

        private void updateSystemsOnPlace(IConduit conduit, int x, int y) {
            int conduitState = conduit.GetState();
            List<TSystem> systemsToMerge = new List<TSystem>();
            Vector2Int position = new Vector2Int(x, y);
            
            PlaceUpdateDirection(position+Vector2Int.left,conduitState,ConduitDirectionState.Left,ConduitDirectionState.Right,systemsToMerge);
            PlaceUpdateDirection(position+Vector2Int.right,conduitState,ConduitDirectionState.Right,ConduitDirectionState.Left,systemsToMerge);
            PlaceUpdateDirection(position+Vector2Int.down,conduitState,ConduitDirectionState.Down,ConduitDirectionState.Up,systemsToMerge);
            PlaceUpdateDirection(position+Vector2Int.up,conduitState,ConduitDirectionState.Up,ConduitDirectionState.Down,systemsToMerge);
            
            IConduitSystem newSystem = ConduitSystemFactory.Create(conduit,this);
            if (newSystem is not TSystem system) {
                Debug.LogError("Tried to add invalid system to conduit manager");
                return;
            }
            newSystem.AddConduit(conduit);
            if (systemsToMerge.Count == 0) { // No systems to merge, create new system
                conduitSystems.Add(system);
                return;
            }
            TSystem mergeInto = systemsToMerge[0];
            mergeInto.Merge(newSystem);
            for (int i = 1; i < systemsToMerge.Count; i++) {
                TSystem toMerge = systemsToMerge[i];
                if (toMerge == null || toMerge.Equals(mergeInto)) {
                    continue;
                }
                mergeInto.Merge(toMerge);
                conduitSystems.Remove(toMerge);
            }
            mergeInto.Rebuild();
        }

        private void PlaceUpdateDirection(Vector2Int directionalPosition, int state, ConduitDirectionState directionState, ConduitDirectionState inverseDirectionState, List<TSystem> mergeSystems)
        {
            bool connects = (state & (int)directionState) != 0;
            if (!connects)
            {
                return;
            }
            IConduit conduit = GetIConduitAtCellPosition(directionalPosition);
            conduit.AddStateDirection(inverseDirectionState);
            
            mergeSystems.Add((TSystem)conduit.GetConduitSystem());
            RefreshConduitTile(conduit);
        }

        private void removeConduitFromSystem(IConduit conduit, int x, int y) {
            // Step 1, delete conduit system
            TSystem conduitSystem = (TSystem) conduit.GetConduitSystem();
            if (conduitSystem == null) {
                Debug.LogError("Conduit somehow didn't belong to a conduit system");
            }
            conduitSystems.Remove(conduitSystem);
            // Step 2, Run DFS on conduit to remove setting the conduit system of all connecting conduits to null
            DFSConduit(conduit,null);

            // Step 3, delete the conduit from the conduit array
            Vector2Int position = new Vector2Int(x, y);
            if (conduits.ContainsKey(position))
            {
                conduits.Remove(position);
            }
            
            // Step 4, Regenerate systems by running DFSConduit on up, left, down, right, and rebuild connections
            IConduit right = GetIConduitAtCellPosition(position+Vector2Int.right);
            RemoveConduitUpdate(right,ConduitDirectionState.Left);
            
            IConduit left = GetIConduitAtCellPosition(position+Vector2Int.left);
            RemoveConduitUpdate(left,ConduitDirectionState.Right);
            
            IConduit up = GetIConduitAtCellPosition(position+Vector2Int.up);
            RemoveConduitUpdate(up,ConduitDirectionState.Down);
            
            IConduit down = GetIConduitAtCellPosition(position+Vector2Int.down);
            RemoveConduitUpdate(down,ConduitDirectionState.Up);
            
        }

        private void RemoveConduitUpdate(IConduit conduit,ConduitDirectionState connectingDirectionState)
        {
            if (conduit == null || !conduit.ConnectsDirection(connectingDirectionState)) return;
            
            conduit.RemoveStateDirection(connectingDirectionState);
            RefreshConduitTile(conduit);

            if (conduit.GetConduitSystem() != null) return;
            TSystem system = (TSystem)ConduitSystemFactory.Create(conduit,this);
            conduitSystems.Add(system);
            DFSConduit(conduit,system);
            RefreshSystemTiles(system);
        }
        

        public IConduit[,] GetConduitPartitionData(Vector2Int partitionPosition) {
            IConduit[,] partitionConduits = new IConduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            Vector2Int position = partitionPosition*Global.ChunkPartitionSize-referencePosition;
            if (position.x < 0 || position.x > size.x || position.y < 0 || position.y > size.y) {
                Debug.LogError("Attempted to get partition conduit data out of range");
                return null;
            }
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++)
                {
                    partitionConduits[x, y] = GetIConduitAtCellPosition(position + new Vector2Int(x, y));
                }
            }
            return partitionConduits;
        }

        public ITileEntityInstance GetTileEntityAtPosition(int x, int y) {
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
        public EntityPortType GetPortTypeAtPosition(int x, int y) {
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
        
        private void generateSystemsFromArray() {
            HashSet<IConduit> conduitsNotSeen = new HashSet<IConduit>();
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    IConduit conduit = GetIConduitAtCellPosition(new Vector2Int(x,y));
                    if (conduit == null || conduit.GetConduitItem() == null) {
                        continue;
                    }
                    conduitsNotSeen.Add(conduit);
                }
            }
            while (conduitsNotSeen.Count > 0) {
                IConduit conduit = conduitsNotSeen.First();
                conduitsNotSeen.Remove(conduit);
                if (conduit.GetConduitSystem() != null) {
                    continue;
                }
                TSystem conduitSystem = (TSystem)ConduitSystemFactory.Create(conduit,this);
                conduitSystems.Add(conduitSystem);

                DFSConduit(conduit, conduitSystem); // Search Array for all connecting conduits
            }
            onGenerationCompleted();
        }

        public abstract void onGenerationCompleted();

        private void DFSConduit(IConduit conduit,IConduitSystem conduitSystem) {
            if (conduit == null)
            {
                return;
            }

            HashSet<IConduit> seen = new HashSet<IConduit>();
            Stack<IConduit> stack = new Stack<IConduit>();
            
            Vector2Int[] directions = {Vector2Int.left, Vector2Int.right,Vector2Int.up,Vector2Int.down};
            ConduitDirectionState[] stateDirections = {ConduitDirectionState.Right,ConduitDirectionState.Left, ConduitDirectionState.Down, ConduitDirectionState.Up};
            stack.Push(conduit);

            while (stack.Count > 0)
            {
                IConduit currentConduit = stack.Pop();

                bool visited = !seen.Add(currentConduit);
                if (visited)
                {
                    continue;
                }

                if (conduitSystem == null)
                {
                    currentConduit.SetConduitSystem(null);
                }
                else
                {
                    if (currentConduit.GetConduitItem().id != conduitSystem.GetId())
                    {
                        continue;
                    }
                    conduitSystem.AddConduit(currentConduit);
                }

                Vector2Int conduitPosition = currentConduit.GetPosition();
                for (int i = 0; i < 4; i++)
                {
                    IConduit adjConduit = GetIConduitAtCellPosition(conduitPosition+directions[i]);
                    if (adjConduit == null || !adjConduit.ConnectsDirection(stateDirections[i])) continue;
                    stack.Push(adjConduit);
                }
            }
        }
        
        

        public ConduitType GetConduitType()
        {
            return type;
        }

        public Dictionary<ITileEntityInstance, List<TileEntityPort>> GetTileEntityPorts()
        {
            return tileEntityConduitPorts;
        }
    }
}