using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits.Ports;
using TileEntity;
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
        public Dictionary<ITileEntityInstance, List<TileEntityPortData>> GetTileEntityPorts();
        public EntityPortType GetPortTypeAtPosition(int x, int y);
        public void AddTileEntity(ITileEntityInstance tileEntity);
        public void DeleteTileEntity(Vector2Int position);
        public int GetNewState(Vector2Int position, ConduitPlacementMode placementMode, string id);
        public void RefreshSystemTiles(IConduitSystem conduitSystem);
        public void RefreshConduitTile(IConduit conduit);
        public void SetTileMap(ConduitTileMap conduitTileMap);
        public IConduit[,] GetConduitPartitionData(Vector2Int partitionPosition);
        public IConduit GetConduitAtCellPosition(Vector2Int position);
        public void ConduitJoinUpdate(IConduit conduit, IConduit adj);
        public void ConduitDisconnectUpdate(IConduit conduit, IConduit adj);
    }

    public interface ITickableConduitSystem {
        public void tickUpdate();
    }
    public abstract class ConduitSystemManager<TConduit, TSystem> : IConduitSystemManager where TConduit : IConduit where TSystem : IConduitSystem {
        protected List<TSystem> conduitSystems;
        protected ConduitType type;
        protected Dictionary<Vector2Int, TConduit> conduits;
        protected Dictionary<ITileEntityInstance, List<TileEntityPortData>> chunkConduitPorts;
        protected ConduitTileMap ConduitTileMap;

        public ConduitType Type { get => type;}
        public Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityConduitPorts { get => chunkConduitPorts; set => chunkConduitPorts = value; }

        protected ConduitSystemManager(
            ConduitType conduitType, 
            Dictionary<Vector2Int, TConduit> conduits, 
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> chunkConduitPorts
            ) {
            this.type = conduitType;
            this.conduits = conduits;
            this.tileEntityConduitPorts = chunkConduitPorts;
            conduitSystems = new List<TSystem>();
            BuildSystems();
        }
        public IConduit GetConduitAtCellPosition(Vector2Int position)
        {
            return conduits.GetValueOrDefault(position);
        }
        
        public void AddTileEntity(ITileEntityInstance tileEntity) {
            if (tileEntity is not IConduitPortTileEntity conduitPortTileEntity) {
                return;
            }
            ConduitPortLayout layout = conduitPortTileEntity.GetConduitPortLayout();
            if (ReferenceEquals(layout,null)) {
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (TileEntityPortData port in chunkConduitPorts[tileEntity])
            {
                Vector2Int position = port.position + tileEntity.getCellPosition();
                if (!conduits.TryGetValue(position, out var conduit))
                {
                    continue;
                }
                if (conduit == null) {
                    continue;
                }
                OnTileEntityAdd(conduit,tileEntity,port);
            }
        }

        public abstract void OnTileEntityAdd(TConduit conduit,ITileEntityInstance tileEntity, TileEntityPortData portData);
        public abstract void OnTileEntityRemoved(TConduit conduit);
        public void DeleteTileEntity(Vector2Int position) {
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPortData>> kvp in chunkConduitPorts) {
                if (kvp.Key.getCellPosition() == position) {
                    foreach (TileEntityPortData port in chunkConduitPorts[kvp.Key])
                    {
                        Vector2Int portPosition = port.position + position;
                        TConduit conduit = (TConduit)GetConduitAtCellPosition(portPosition);
                        if (conduit == null) {
                            continue;
                        }
                        OnTileEntityRemoved(conduit);
                    }
                    tileEntityConduitPorts.Remove(kvp.Key);
                    return;
                }
            }
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
            IConduit left = GetConduitAtCellPosition(position+Vector2Int.left);
            int state = 0;
            if (left != null && left.GetId() == id)
            {
                state += (int)ConduitDirectionState.Left;
            }
            IConduit right = GetConduitAtCellPosition(position+Vector2Int.right);
            if (right != null && right.GetId() == id)
            {
                state += (int)ConduitDirectionState.Right;
            }
            IConduit up = GetConduitAtCellPosition(position+Vector2Int.up);
            if (up != null && up.GetId() == id)
            {
                state += (int)ConduitDirectionState.Up;
            }
            IConduit down = GetConduitAtCellPosition(position+Vector2Int.down);
            if (down != null && down.GetId() == id)
            {
                state += (int)ConduitDirectionState.Down;
            }

            return state;
        }

        public void RefreshSystemTiles(IConduitSystem conduitSystem)
        {
            bool noTileMapLoaded = !ConduitTileMap;
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
            bool noTileMapLoaded = !ConduitTileMap;
            if (noTileMapLoaded)
            {
                return;
            }
            ConduitTileMap.RefreshTile(conduit.GetX(), conduit.GetY());
        }

        public void SetTileMap(ConduitTileMap conduitTileMap)
        {
            this.ConduitTileMap = conduitTileMap;
        }
        

        public void SetConduit(int x, int y, IConduit conduit) {
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
            IConduit conduit = GetConduitAtCellPosition(directionalPosition);
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
            BfsConduit(conduit,null);

            // Step 3, delete the conduit from the conduit array
            Vector2Int position = new Vector2Int(x, y);
            if (conduits.ContainsKey(position))
            {
                conduits.Remove(position);
            }
            
            // Step 4, Regenerate systems by running BfsConduit on up, left, down, right, and rebuild connections
            IConduit right = GetConduitAtCellPosition(position+Vector2Int.right);
            RemoveConduitUpdate(right,ConduitDirectionState.Left);
            
            IConduit left = GetConduitAtCellPosition(position+Vector2Int.left);
            RemoveConduitUpdate(left,ConduitDirectionState.Right);
            
            IConduit up = GetConduitAtCellPosition(position+Vector2Int.up);
            RemoveConduitUpdate(up,ConduitDirectionState.Down);
            
            IConduit down = GetConduitAtCellPosition(position+Vector2Int.down);
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
            BfsConduit(conduit,system);
            RefreshSystemTiles(system);
        }

        public void ConduitJoinUpdate(IConduit conduit, IConduit adj)
        {
            BfsConduit(conduit,conduit.GetConduitSystem());
        }

        public void ConduitDisconnectUpdate(IConduit conduit, IConduit adj)
        {
            IConduitSystem conduitSystem = conduit.GetConduitSystem();
            BfsConduit(conduit,null);
            bool stillConnected = adj.GetConduitSystem() == null;
            if (stillConnected)
            {
                BfsConduit(conduit,conduitSystem);
                return;
            }
            conduitSystems.Remove((TSystem)conduitSystem);
            
            IConduitSystem firstNew = ConduitSystemFactory.Create(conduit,this);
            BfsConduit(conduit,firstNew);
            conduitSystems.Add((TSystem)firstNew);
            
            IConduitSystem secondNew = ConduitSystemFactory.Create(adj,this);
            BfsConduit(adj,secondNew);
            conduitSystems.Add((TSystem)secondNew);
            
        }
        

        public IConduit[,] GetConduitPartitionData(Vector2Int partitionPosition) {
            IConduit[,] partitionConduits = new IConduit[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            Vector2Int position = partitionPosition * Global.ChunkPartitionSize;
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++)
                {
                    partitionConduits[x, y] = GetConduitAtCellPosition(position + new Vector2Int(x, y));
                }
            }
            return partitionConduits;
        }

        public ITileEntityInstance GetTileEntityAtPosition(int x, int y) {
            // TODO make this more efficent
            Vector2Int pos = new Vector2Int(x,y);
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPortData>> kvp in chunkConduitPorts) {
                //Vector2Int distVec = kvp.Key - new Vector2Int(x,y);
                foreach (TileEntityPortData tileEntityPort in kvp.Value) {
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
            foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPortData>> kvp in chunkConduitPorts) {
                //Vector2Int distVec = kvp.Key - new Vector2Int(x,y);
                foreach (TileEntityPortData tileEntityPort in kvp.Value) {
                    // Key is tileEntityPosition
                    if (tileEntityPort.position + kvp.Key.getCellPosition() == pos) { // Conduit is placed on tileEntityPort
                        return tileEntityPort.portType;
                    }
                }
            }
            return EntityPortType.None;
        }
        
        private void BuildSystems()
        {
            foreach (var (position, conduit) in conduits)
            {
                if (conduit.GetConduitSystem() != null) {
                    continue;
                }
                TSystem conduitSystem = (TSystem)ConduitSystemFactory.Create(conduit,this);
                conduitSystems.Add(conduitSystem);
                BfsConduit(conduit, conduitSystem);
            }
            OnGenerationCompleted();
        }

        public abstract void OnGenerationCompleted();

        private void BfsConduit(IConduit conduit,IConduitSystem conduitSystem) {
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
                    if (currentConduit.GetConduitItem().id != conduitSystem.GetId()) continue;
                    conduitSystem.AddConduit(currentConduit);
                }

                Vector2Int conduitPosition = currentConduit.GetPosition();
                for (int i = 0; i < 4; i++)
                {
                    IConduit adjConduit = GetConduitAtCellPosition(conduitPosition+directions[i]);
                    if (adjConduit == null || !adjConduit.ConnectsDirection(stateDirections[i])) continue;
                    stack.Push(adjConduit);
                }
            }
        }
        
        public ConduitType GetConduitType()
        {
            return type;
        }

        public Dictionary<ITileEntityInstance, List<TileEntityPortData>> GetTileEntityPorts()
        {
            return tileEntityConduitPorts;
        }
    }
}