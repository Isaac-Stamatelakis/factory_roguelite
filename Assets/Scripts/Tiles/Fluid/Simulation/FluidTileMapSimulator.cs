using System;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Fluids;
using Items;
using TileMaps;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Analytics;

namespace Tiles.Fluid.Simulation
{
	public enum FluidFlowDirection
	{
		Up = 1,
		Left = 2,
		Right = 4,
		Down = 8
	}
	public class FluidCell
	{
		public FluidTileItem FluidTileItem;
		public float Liquid;
		public int FlowBitMap;
		public Vector2Int Position;
		public float Diff;
		public bool QueuedForUpdate;
		public bool Displayable;

		public bool CanFlow(FluidFlowDirection fluidFlowDirection)
		{
			return (FlowBitMap & (int)fluidFlowDirection) != 0;
		}

		public FluidCell(FluidTileItem fluidTileItem, float liquid, int flowBitMap, Vector2Int position, bool displayable)
		{
			FluidTileItem = fluidTileItem;
			Liquid = liquid;
			this.FlowBitMap = flowBitMap;
			Position = position;
			Displayable = displayable;
		}
	}
	/*
	 * Adapted from https://github.com/jongallant/LiquidSimulator/tree/master
	 * With massive changes and improvements
	 */

	public class FluidUpdateCollection
	{
		public Vector2Int[] Updates;
		public int Index;

		public FluidUpdateCollection(Vector2Int[] updates, int index)
		{
			Updates = updates;
			Index = index;
		}
	}
	
    public class FluidTileMapSimulator
    {
	    private const bool LOG = false;
	    
	    public FluidTileMapSimulator(FluidTileMap fluidTileMap, WorldTileMap objectTileMap, WorldTileMap blockTileMap)
	    {
		    this.fluidTileMap = fluidTileMap;
		    this.objectTileMap = objectTileMap;
		    this.blockTileMap = blockTileMap;
		    currentUpdates = new FluidCell[MAX_UPDATES_PER_SECOND];
		    for (int i = 0; i < STACKS; i++)
		    {
			    Vector2Int[] stackUpdateList = new Vector2Int[MAX_UPDATES_PER_SECOND];
			    FluidUpdateCollection fluidUpdateCollection = new FluidUpdateCollection(stackUpdateList, 0);
			    fluidUpdateArrayStack.Push(fluidUpdateCollection);
		    }
	    }
	    
	    private uint ticks;
	    private uint tickCounter;
	    private uint tickArrayCounter;
	    private Dictionary<uint, FluidUpdateCollection> tickFluidUpdates = new (); 
	    private Stack<FluidUpdateCollection> fluidUpdateArrayStack = new (STACKS);
	    private Dictionary<Vector2Int, FluidCell[][]> chunkCellArrayDict = new();
	    private FluidCell[] currentUpdates;
	    
	    // Note: These values take up this is only 640kB
	    const int MAX_UPDATES_PER_SECOND = 2048;
	    private const int STACKS = 60;
	    const float MAX_FILL = 1.0f;
	    const float MIN_FILL = 0.025f;
	    
	    #if UNITY_EDITOR
	    private bool stackEmpty = false;
	    private bool updateListFull = false;
	    #endif

	    // Extra liquid a cell can store than the cell above it
	    const float MAX_COMPRESSION = 0.0f;

	    // Lowest and highest amount of liquids allowed to flow per iteration
	    const float MIN_FLOW = 0.005f;
	    const float MAX_FLOW = 4f;

	    // Adjusts flow speed (0.0f - 1.0f)
	    const float FLOW_SPEED = 1f;
	    private FluidTileMap fluidTileMap;
	    private WorldTileMap objectTileMap;
	    private WorldTileMap blockTileMap;
        float CalculateVerticalFlowValue(float remainingLiquid, FluidCell destination)
        {
	        float sum = remainingLiquid + destination.Liquid;

	        if (sum <= MAX_FILL)
		        return MAX_FILL;
	        if (sum < 2 * MAX_FILL + MAX_COMPRESSION)
		        return (MAX_FILL * MAX_FILL + sum * MAX_COMPRESSION) / (MAX_FILL + MAX_COMPRESSION);
	        return (sum + MAX_COMPRESSION) / 2f;
        }

        public void AddChunk(Vector2Int position, FluidCell[][] fluidCells)
        {
	        chunkCellArrayDict[position] = fluidCells;
	        foreach (FluidCell[] fluidCellArray in fluidCells)
	        {
		        foreach (FluidCell cell in fluidCellArray)
		        {
			        if (cell == null) continue;
			        
			        bool CheckSimilarity(FluidCell adjacent, bool top)
			        {
				        bool adjacentNull = adjacent == null;
				        if (adjacentNull && top) return true;
				        if (adjacentNull || (Mathf.Abs(adjacent.Liquid - cell.Liquid) < MIN_FILL)) return false;
				        UnsettleCell(cell);
				        return true;
				       
			        }
			       
			        if (CheckSimilarity(GetFluidCell(cell.Position + Vector2Int.left),false)) continue;
			        if (CheckSimilarity(GetFluidCell(cell.Position + Vector2Int.right),false)) continue;
			        if (CheckSimilarity(GetFluidCell(cell.Position + Vector2Int.down),false)) continue;
			        if (CheckSimilarity(GetFluidCell(cell.Position + Vector2Int.up),true)) continue;
		        }
	        }
        }

        public void RemoveChunk(Vector2Int position)
        {
	        chunkCellArrayDict.Remove(position);
        }

        public void SetPartitionDisplayStatus(Vector2Int partitionPosition, bool display)
        {
	        Vector2Int chunkPosition = Global.GetChunkFromCell(partitionPosition * Global.CHUNK_PARTITION_SIZE);
	        if (!chunkCellArrayDict.TryGetValue(chunkPosition, out FluidCell[][] fluidCells)) return;
	        Vector2Int partitionPositionInChunk = Global.CHUNK_PARTITION_SIZE * (partitionPosition - chunkPosition * Global.PARTITIONS_PER_CHUNK);
	       
	        for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
	        {
		        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
		        {
			        var cell = fluidCells[x + partitionPositionInChunk.x][y + partitionPositionInChunk.y];
			        if (cell == null) continue;
			        cell.Displayable = display;
			        if (display) DisplayCell(cell);
		        }
	        }
        }
        public void SaveToChunk(ILoadedChunk loadedChunk)
        {
	        Vector2Int chunkPosition = loadedChunk.GetPosition();
	        if (!chunkCellArrayDict.TryGetValue(chunkPosition, out FluidCell[][] fluidCells)) return;
	        for (int px = 0; px < Global.PARTITIONS_PER_CHUNK; px++)
	        {
		        for (int py = 0; py < Global.PARTITIONS_PER_CHUNK; py++)
		        {
			        IChunkPartition partition = loadedChunk.GetPartition(new Vector2Int(px, py));
			        PartitionFluidData fluidData = partition.GetFluidData();
			        string[,] ids = fluidData.ids;
			        float[,] fill = fluidData.fill;
			        for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
			        {
				        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
				        {
					        FluidCell cell = fluidCells[px*Global.CHUNK_PARTITION_SIZE + x][py*Global.CHUNK_PARTITION_SIZE + y];
					        if (cell == null)
					        {
						        ids[x, y] = null;
						        fill[x, y] = 0;
					        }
					        else
					        {
						        ids[x, y] = cell.FluidTileItem?.id;
						        fill[x, y] = cell.Liquid;
					        }
				        }
			        }
		        }
	        }
        }

        public void DisruptSurface(Vector2Int cellPosition)
        {
	        FluidCell cell = GetFluidCell(cellPosition);
	        if (cell == null || cell.Liquid < MIN_FILL) return;
	        
	        float current = cell.Liquid;
	        bool Disrupt(FluidCell adjacent)
	        {
		        if (adjacent == null || !(adjacent.Liquid > MIN_FILL) || adjacent.Liquid > MAX_FILL- 0.05f) return false;
		        float dif = UnityEngine.Random.Range(current / 4, current / 2);
		        if (adjacent.Liquid + dif > MAX_FILL)
		        {
			        dif = adjacent.Liquid + dif - MAX_FILL;
		        }
		        adjacent.Liquid += dif;
		        cell.Liquid -= dif;
		        return true;
	        }
	        FluidCell left = GetFluidCell(cellPosition + Vector2Int.left);
	        bool disruptLeft = Disrupt(left);
	        FluidCell right = GetFluidCell(cellPosition + Vector2Int.right);
	        bool disruptRight = Disrupt(right);
	        if (!disruptLeft && !disruptRight)
	        {
		        FluidCell top = GetFluidCell(cellPosition + Vector2Int.up);
		        bool disruptTop = Disrupt(top);
		        if (!disruptTop) return;
	        }
	        
	        UnsettleCell(cell);
	        UnsettleNeighbors(cell.Position);

        }
        public void AddFluidCell(FluidCell fluidCell, bool replace)
        {
	        Vector2Int chunkPosition = Global.GetChunkFromCell(fluidCell.Position);
	        if (!chunkCellArrayDict.TryGetValue(chunkPosition, out FluidCell[][] fluidCells)) return;
	        Vector2Int positionInChunk = fluidCell.Position - chunkPosition * Global.CHUNK_SIZE;
	        FluidCell current = fluidCells[positionInChunk.x][positionInChunk.y];
	        if (current == null || replace)
	        {
		        fluidCells[positionInChunk.x][positionInChunk.y] = fluidCell;
	        }
	        
	        UnsettleCell(fluidCell);
	        UnsettleNeighbors(fluidCell.Position);
        }
        
        public void RemoveFluidCell(Vector2Int position)
        {
	        Vector2Int chunkPosition = Global.GetChunkFromCell(position);
	        if (!chunkCellArrayDict.TryGetValue(chunkPosition, out FluidCell[][] fluidCells)) return;
	        Vector2Int positionInChunk = position - chunkPosition * Global.CHUNK_SIZE;
	        fluidCells[positionInChunk.x][positionInChunk.y] = null;
        }
        
        private void DisplayCell(FluidCell fluidCell)
        {
	        if (!fluidCell.Displayable) return;
	        var above = GetFluidCell(fluidCell.Position + Vector2Int.up);
	        if (above != null && above.Liquid > 1/16f && above.FluidTileItem)
	        {
		        fluidTileMap.DisplayTile(fluidCell.Position.x,fluidCell.Position.y, fluidCell.FluidTileItem, 1);
		        return;
	        }
	        if (fluidCell.Liquid > MIN_FILL)
	        {
		        fluidTileMap.DisplayTile(fluidCell);
		        return;
	        }
	        fluidCell.Liquid = 0;
	        
	        fluidTileMap.DisplayTile(fluidCell);
	        fluidCell.FluidTileItem = null; // Set tile after filling for material access
        }
		public void Simulate()
		{
			tickArrayCounter = tickCounter;
			bool update = tickFluidUpdates.TryGetValue(tickCounter, out var fluidUpdateCollection);
			if (update) tickFluidUpdates.Remove(tickCounter);
			tickCounter++;
			if (!update) return;
			
			var updates = fluidUpdateCollection.Updates;
			int updateIndex = 0;
			for (var i = 0; i < updates.Length; i++)
			{
				var position = updates[i];
				FluidCell fluidCell = GetFluidCell(position);
				if (fluidCell == null) continue;
				currentUpdates[updateIndex] = fluidCell;
				fluidCell.QueuedForUpdate = false;
				updateIndex++;
				if (updateIndex >= fluidUpdateCollection.Index) break;
			}

			foreach (var fluidCell in currentUpdates)
			{
				if (fluidCell == null) break;
				fluidCell.Diff = 0;
			}

			for (var index = 0; index < currentUpdates.Length; index++)
			{
				FluidCell cell = currentUpdates[index];
				if (cell == null) break;
				
				FluidUpdate(cell);
			}
			
			void UpdateFill(FluidCell adjacent)
			{
				if (adjacent == null || adjacent.Diff == 0) return;
				bool emptyBefore = adjacent.Liquid < MIN_FILL;
				adjacent.Liquid += adjacent.Diff;
				if (emptyBefore && adjacent.Liquid > MIN_FILL)
				{
					objectTileMap.FluidUpdate(adjacent.Position);
				}
				adjacent.Diff = 0;
				if (adjacent.Liquid > MAX_FILL) adjacent.Liquid = MAX_FILL;
				DisplayCell(adjacent);
			}
			
			for (var index = 0; index < currentUpdates.Length; index++)
			{
				var cell = currentUpdates[index];
				currentUpdates[index] = null;
				if (cell == null) break;
				
				Vector2Int cellPosition = cell.Position;
				UpdateFill(cell);
				UpdateFill(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Left)));
				UpdateFill(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Right)));
				UpdateFill(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Up)));
				UpdateFill(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Down)));
				DisplayCell(cell);
			}

#pragma warning disable 0162
			if (LOG)
			{
				Debug.Log($"Simulator Updated {updateIndex} Cells at Tick {tickCounter}");
			}
#pragma warning restore 0162
			
			fluidUpdateCollection.Index = 0;
			fluidUpdateArrayStack.Push(fluidUpdateCollection);
				
			#if UNITY_EDITOR
			if (updateListFull)
			{
				Debug.LogWarning("Fluid update list is full, pushed updates to next tick");
				updateListFull = false;
			}

			if (stackEmpty)
			{
				Debug.LogWarning("Fluid update list stack is empty");
				stackEmpty = false;
			}
			#endif
		}

		private void FluidUpdate(FluidCell cell)
		{
			if (cell.Liquid < MIN_FILL || !cell.FluidTileItem) {
				cell.Liquid = 0;
				return;
			}
			
			float startValue = cell.Liquid;
			float remainingValue = cell.Liquid;
			float flow = 0;
			
			if (!cell.FluidTileItem) return;
			
			FluidFlowDirection verticalFlowDirection = cell.FluidTileItem.fluidOptions.InvertedGravity 
				? FluidFlowDirection.Up 
				: FluidFlowDirection.Down;
			if (cell.CanFlow(verticalFlowDirection))
			{
				FallFlowUpdate(cell,verticalFlowDirection,ref remainingValue, ref flow);
			}
			
			
			const int HORIZONTAL_DISTANCE = 2;
			bool left = cell.CanFlow(FluidFlowDirection.Left);
			bool right = cell.CanFlow(FluidFlowDirection.Right);
			for (int distance = 1; distance <= HORIZONTAL_DISTANCE; distance++)
			{
				if (left)
				{
					
					left = HorizontalFlowUpdate(cell,FluidFlowDirection.Left,ref remainingValue, ref flow,distance);
				}
				
				if (right)
				{
					right = HorizontalFlowUpdate(cell,FluidFlowDirection.Right,ref remainingValue, ref flow,distance);
				}
				if (!left && !right) break;
			}

			if (!cell.FluidTileItem) return;
			FluidFlowDirection riseFlowDirection = cell.FluidTileItem.fluidOptions.InvertedGravity
				? FluidFlowDirection.Down
				: FluidFlowDirection.Up;
			if (cell.CanFlow(riseFlowDirection))
			{
				RiseFluidUpdate(cell,riseFlowDirection,ref remainingValue, ref flow);
			}
			
			
			if (remainingValue < MIN_FILL) {
				cell.Diff -= remainingValue;
			}
			// Check if cell is settled
			if (Mathf.Abs(startValue - remainingValue) > 0.001f) {
				UnsettleCell(cell);
				UnsettleNeighbors(cell.Position); 
			}
		}

		public void FallFlowUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow)
		{
			if (remainingValue < MIN_FILL) return;
			FluidCell adjacent = GetFluidCellInDirection(cell, flowDirection);
			if (!CanFlowInto(cell, adjacent, flowDirection,ref remainingValue)) return;
			
			flow = CalculateVerticalFlowValue(cell.Liquid, adjacent) - adjacent.Liquid;
			if (adjacent.Liquid > 0 && flow > MIN_FLOW)
				flow *= FLOW_SPEED; 
			
			flow = Mathf.Max (flow, 0);
			if (flow > Mathf.Min(MAX_FLOW, cell.Liquid)) 
				flow = Mathf.Min(MAX_FLOW, cell.Liquid);
			
			UpdateFlowValues(ref remainingValue, flow, cell, adjacent,false);
		}
		
		private void UnsettleCell(FluidCell adjacent)
		{
			if (adjacent == null || adjacent.QueuedForUpdate) return;
			FluidTileItem fluidTileItem = adjacent.FluidTileItem;
			if (!fluidTileItem) return;

			uint updateTick = (uint)fluidTileItem.fluidOptions.Viscosity + tickArrayCounter;
			if (!tickFluidUpdates.ContainsKey(updateTick))
			{
				if (fluidUpdateArrayStack.Count == 0)
				{
					#if UNITY_EDITOR
					stackEmpty = true;
					#endif
					return;
				}
				tickFluidUpdates[updateTick] = fluidUpdateArrayStack.Pop();
			}
			
			var collection = tickFluidUpdates[updateTick];

			if (collection.Index >= MAX_UPDATES_PER_SECOND)
			{
				tickArrayCounter++;
				UnsettleCell(adjacent);
				#if UNITY_EDITOR
				updateListFull = true;
				#endif
				return;
			}
			adjacent.QueuedForUpdate = true;
			collection.Updates[collection.Index] = adjacent.Position;
			collection.Index++;
		}
		
		public void UnsettleNeighbors(Vector2Int cellPosition)
		{
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.up));
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.down));
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.left));
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.right));
		}

		public FluidFlowDirection InvertFlowDirection(FluidFlowDirection flowDirection)
		{
			switch (flowDirection)
			{
				case FluidFlowDirection.Up:
					return FluidFlowDirection.Down;
				case FluidFlowDirection.Left:
					return FluidFlowDirection.Right;
				case FluidFlowDirection.Right:
					return FluidFlowDirection.Left;
				case FluidFlowDirection.Down:
					return FluidFlowDirection.Up;
				default:
					throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
			}
		}

		public bool CanFlowInto(FluidCell fluidCell, FluidCell adj, FluidFlowDirection flowDirection, ref float remainingFluid)
		{
			if (adj == null || !adj.CanFlow(InvertFlowDirection(flowDirection))) return false;
			if (!adj.FluidTileItem || adj.FluidTileItem.id == fluidCell.FluidTileItem?.id || adj.Liquid < MIN_FILL) return true;
			FluidTileItem cellItem = fluidCell.FluidTileItem;
			if (!cellItem) return false;
			FluidTileItem adjItem = adj.FluidTileItem;
			if (!adjItem) return false;
			if (cellItem.fluidOptions.CollisionDominance == adjItem.fluidOptions.CollisionDominance) return false;
			FluidCell dominator = cellItem.fluidOptions.CollisionDominance < adjItem.fluidOptions.CollisionDominance ? adj : fluidCell;
			FluidTileItem dominatorItem = cellItem.fluidOptions.CollisionDominance < adjItem.fluidOptions.CollisionDominance ? adjItem : cellItem;
			TileItem dominatorTile = dominatorItem.fluidOptions.OnCollisionTile;
			if (!dominatorTile) return false;
			FluidCell dominated = cellItem.fluidOptions.CollisionDominance > adjItem.fluidOptions.CollisionDominance ? adj : fluidCell;
			dominator.Diff = 0;
			dominator.Liquid = 0;
			remainingFluid = 0;
			dominator.FluidTileItem = null;
			dominated.Liquid = 0;
			
			blockTileMap.PlaceNewTileAtLocation(dominated.Position.x,dominated.Position.y,dominatorTile);
			fluidTileMap.BreakTile(dominated.Position);
			fluidTileMap.BreakTile(dominator.Position);
			UnsettleNeighbors(dominated.Position);
	
			return false;
		}
		public void UpdateFlowValues(ref float remainingValue, float flow, FluidCell cell, FluidCell adjacent, bool loss)
		{
			if (flow == 0) return;
			remainingValue -= flow;
			cell.Diff -= flow;
			// flow > 0.005f
			if (!loss || flow > 0.000f) // This makes fluids lose size as they move but makes 
			{
				adjacent.Diff += flow;
			}
			adjacent.FluidTileItem = cell.FluidTileItem;
		}

		public bool HorizontalFlowUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow, int distance)
		{
			if (remainingValue < MIN_FILL) return false;
			FluidCell adjacent = GetFluidCell(cell.Position + GetVectorDirectionFromFlow(flowDirection) * distance);
			if (!CanFlowInto(cell, adjacent,flowDirection, ref remainingValue)) return false;
			
			flow = (remainingValue - adjacent.Liquid)/4;
			
			if (flow > MIN_FLOW)
				flow *= FLOW_SPEED;
			
			flow = Mathf.Max(flow, 0);
			float currentMax = Mathf.Min(MAX_FLOW, remainingValue);
			if (flow > currentMax)
				flow = currentMax;

			UpdateFlowValues(ref remainingValue, flow, cell, adjacent,true);
			return adjacent.CanFlow(flowDirection);
		}

		public void RiseFluidUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow)
		{
			if (remainingValue < MIN_FILL) return;
			FluidCell adjacent = GetFluidCellInDirection(cell, flowDirection);
			if (!CanFlowInto(cell, adjacent, flowDirection,ref remainingValue)) return;
			
			flow = remainingValue - CalculateVerticalFlowValue (remainingValue, adjacent); 
			if (flow > MIN_FLOW)
				flow *= FLOW_SPEED; 
			
			flow = Mathf.Max (flow, 0);
			if (flow > Mathf.Min(MAX_FLOW, remainingValue)) 
				flow = Mathf.Min(MAX_FLOW, remainingValue);
			
			UpdateFlowValues(ref remainingValue, flow, cell, adjacent,false);
		}
		
		public FluidCell GetFluidCell(Vector2Int cellPosition)
		{
			Vector2Int chunkPosition = Global.GetChunkFromCell(cellPosition);
			if (!chunkCellArrayDict.TryGetValue(chunkPosition, out FluidCell[][] fluidCells))return null;
			Vector2Int positionInChunk = cellPosition - chunkPosition * Global.CHUNK_SIZE;
			return fluidCells[positionInChunk.x][positionInChunk.y];
		}

		public FluidCell GetFluidCellInDirection(FluidCell fluidCell, FluidFlowDirection flowDirection)
		{
			return GetFluidCell(fluidCell.Position + GetVectorDirectionFromFlow(flowDirection));
		}

		private Vector2Int GetVectorDirectionFromFlow(FluidFlowDirection flowDirection)
		{
			switch (flowDirection)
			{
				case FluidFlowDirection.Up:
					return Vector2Int.up;
				case FluidFlowDirection.Left:
					return Vector2Int.left;
				case FluidFlowDirection.Down:
					return Vector2Int.down;
				case FluidFlowDirection.Right:
					return Vector2Int.right;
				default:
					throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
			}
		}
    }
}
