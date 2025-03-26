using System;
using System.Collections.Generic;
using Fluids;
using Items;
using UnityEditor.UIElements;
using UnityEngine;

namespace Tiles.Fluid.Simulation
{
	public enum FluidFlowDirection
	{
		Up,
		Left,
		Right,
		Down
	}
	public enum FluidFlowRestriction
	{
		BlockFluids,
		WaterLog,
		NoRestriction
	}
	public class FluidCell
	{
		public string FluidId;
		public float Liquid;
		public FluidFlowRestriction FlowRestriction;
		public Vector2Int Position;
		public float Diff;
		public bool QueuedForUpdate;
		public bool UpdateVisuals;

		public FluidCell(string fluidId, float liquid, FluidFlowRestriction flowRestriction, Vector2Int position)
		{
			FluidId = fluidId;
			Liquid = liquid;
			FlowRestriction = flowRestriction;
			Position = position;
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
	    private const int MAX_VISCOSITY = 10;
	    public FluidTileMapSimulator(FluidWorldTileMap fluidWorldTileMap)
	    {
		    this.fluidWorldTileMap = fluidWorldTileMap;
		    currentUpdates = new FluidCell[MAX_UPDATES_PER_SECOND];
		    for (int i = 0; i < MAX_VISCOSITY; i++)
		    {
			    Vector2Int[] stackUpdateList = new Vector2Int[MAX_UPDATES_PER_SECOND];
			    FluidUpdateCollection fluidUpdateCollection = new FluidUpdateCollection(stackUpdateList, 0);
			    fluidUpdateArrayStack.Push(fluidUpdateCollection);
		    }
		    itemRegistry = ItemRegistry.GetInstance();
	    }

	    private ItemRegistry itemRegistry;
	    private uint ticks;
	    private uint tickCounter;
	    private Dictionary<uint, FluidUpdateCollection> tickFluidUpdates = new (); 
	    private Stack<FluidUpdateCollection> fluidUpdateArrayStack = new (MAX_VISCOSITY);
	    private Dictionary<Vector2Int, FluidCell[][]> chunkCellArrayDict = new();
	    private FluidCell[] currentUpdates;
	    const int MAX_UPDATES_PER_SECOND = 1024;
	    const float MAX_FILL = 1.0f;
	    const float MIN_FILL = 0.005f;
	    private bool stackEmpty = false;
	    private bool updateListFull = false;

	    // Extra liquid a cell can store than the cell above it
	    const float MAX_COMPRESSION = 0.25f;

	    // Lowest and highest amount of liquids allowed to flow per iteration
	    const float MIN_FLOW = 0.005f;
	    const float MAX_FLOW = 4f;

	    // Adjusts flow speed (0.0f - 1.0f)
	    const float FLOW_SPEED = 1f;
	    private FluidWorldTileMap fluidWorldTileMap;
	    
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
			        UnsettleCell(cell);
		        }
	        }
        }

        public void RemoveChunk(Vector2Int position)
        {
	        chunkCellArrayDict.Remove(position);
        }
	
        public void AddFluidCell(FluidCell fluidCell)
        {
	        Vector2Int chunkPosition = Global.getChunkFromCell(fluidCell.Position);
	        if (!chunkCellArrayDict.TryGetValue(chunkPosition, out FluidCell[][] fluidCells)) return;
	        Vector2Int positionInChunk = fluidCell.Position - chunkPosition * Global.CHUNK_SIZE;
	        fluidCells[positionInChunk.x][positionInChunk.y] = fluidCell;
	        UnsettleCell(fluidCell);
        }
        
		public void Simulate()
		{
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
				if (fluidCell.QueuedForUpdate) continue;
				fluidCell.QueuedForUpdate = true;
				currentUpdates[updateIndex] = fluidCell;
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

			void UpdateVisualsOfAdjacent(FluidCell adjacent)
			{
				if (adjacent == null) return;
				if (!adjacent.UpdateVisuals || adjacent.QueuedForUpdate) return;
				adjacent.Liquid += adjacent.Diff;
				adjacent.UpdateVisuals = false;
				if (adjacent.Liquid > MIN_FILL)
				{
					fluidWorldTileMap.DisplayTile(adjacent);
					return;
				}

				adjacent.Liquid = 0;
				adjacent.FluidId = null;
				fluidWorldTileMap.DisplayTile(adjacent);
			}
			
			for (var index = 0; index < currentUpdates.Length; index++)
			{
				var cell = currentUpdates[index];
				if (cell == null) break;
				cell.Liquid += cell.Diff;
				//Debug.Log($"{cell.Position} {cell.Liquid}");
				Vector2Int cellPosition = cell.Position;
				UpdateVisualsOfAdjacent(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Left)));
				UpdateVisualsOfAdjacent(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Right)));
				UpdateVisualsOfAdjacent(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Up)));
				UpdateVisualsOfAdjacent(GetFluidCell(cellPosition + GetVectorDirectionFromFlow(FluidFlowDirection.Down)));
				
				if (cell.Liquid > MIN_FILL)
				{
					fluidWorldTileMap.DisplayTile(cell);
					continue;
				}

				cell.Liquid = 0;
				cell.FluidId = null;
				fluidWorldTileMap.DisplayTile(cell);
			}

			for (var index = 0; index < currentUpdates.Length; index++)
			{
				var cell = currentUpdates[index];
				if (cell == null) break;
				cell.QueuedForUpdate = false;
				currentUpdates[index] = null;
			}
			Debug.Log($"Simulator Updated {fluidUpdateCollection.Index} Cells at Tick {tickCounter}");
			fluidUpdateCollection.Index = 0;
			fluidUpdateArrayStack.Push(fluidUpdateCollection);
			
			if (updateListFull)
			{
				Debug.LogWarning("Fluid update list is full");
				updateListFull = false;
			}

			if (stackEmpty)
			{
				Debug.LogWarning("Fluid update list stack is empty");
				stackEmpty = false;
			}
		}

		private void FluidUpdate(FluidCell cell)
		{
			if (cell.FlowRestriction == FluidFlowRestriction.BlockFluids) {
				cell.Liquid = 0;
				return;
			}
			if (cell.Liquid == 0)
				return;
			if (cell.Liquid < MIN_FILL) {
				cell.Liquid = 0;
				return;
			}
			// Keep track of how much liquid this cell started off with
			float startValue = cell.Liquid;
			float remainingValue = cell.Liquid;
			float flow = 0;
			
			
			bool TerminateUpdate()
			{
				if (remainingValue < MIN_FILL) {
					cell.Diff -= remainingValue;
					return true;
				}

				return false;
			}
			
			bool fall = FallFlowUpdate(cell,FluidFlowDirection.Down,ref remainingValue, ref flow); // TODO falling up 
			if (TerminateUpdate()) return;
			
			const int HORIZONTAL_DISTANCE = 2;
			bool left = true;
			bool right = true;
			for (int i = 1; i <= HORIZONTAL_DISTANCE; i++)
			{
				if (left)
				{
					left = HorizontalFlowUpdate(cell,FluidFlowDirection.Left,ref remainingValue, ref flow,i);
					if (TerminateUpdate()) return;
				}
				
				if (right)
				{
					right = HorizontalFlowUpdate(cell,FluidFlowDirection.Right,ref remainingValue, ref flow,i);
					if (TerminateUpdate()) return;
				}
				if (!left && !right) break;
			}

			
			RiseFluidUpdate(cell,FluidFlowDirection.Up,ref remainingValue, ref flow);
			if (TerminateUpdate()) return;
			
			test:
			// Check if cell is settled
			if (startValue != remainingValue) {
				UnsettleCell(cell);
				UnsettleNeighbors(cell.Position); 
			}
		}

		public bool FallFlowUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow)
		{
			FluidCell adjacent = GetFluidCellInDirection(cell, flowDirection);
			if (!CanFlowInto(cell, adjacent)) return true;
			
			flow = CalculateVerticalFlowValue(cell.Liquid, adjacent) - adjacent.Liquid;
			if (adjacent.Liquid > 0 && flow > MIN_FLOW)
				flow *= FLOW_SPEED; 
			
			flow = Mathf.Max (flow, 0);
			if (flow > Mathf.Min(MAX_FLOW, cell.Liquid)) 
				flow = Mathf.Min(MAX_FLOW, cell.Liquid);
			
			UpdateFlowValues(ref remainingValue, flow, cell, adjacent,false);
			return false;
		}
		
		private void UnsettleCell(FluidCell adjacent)
		{
			if (adjacent == null) return;
			FluidTileItem fluidTileItem = itemRegistry.GetFluidTileItem(adjacent.FluidId);
			if (!fluidTileItem) return;

			uint updateTick = (uint)fluidTileItem.fluidOptions.Viscosity + tickCounter;
			if (!tickFluidUpdates.ContainsKey(updateTick))
			{
				if (fluidUpdateArrayStack.Count == 0)
				{
					stackEmpty = true;
					return;
				}
				tickFluidUpdates[updateTick] = fluidUpdateArrayStack.Pop();
			}

			var collection = tickFluidUpdates[updateTick];

			if (collection.Index >= MAX_UPDATES_PER_SECOND)
			{
				updateListFull = true;
				return;
			}
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

		public bool CanFlowInto(FluidCell fluidCell, FluidCell adj)
		{
			return adj != null && adj.FlowRestriction != FluidFlowRestriction.BlockFluids && (adj.FluidId == null || string.Equals(fluidCell.FluidId, adj.FluidId));
		}
		public void UpdateFlowValues(ref float remainingValue, float flow, FluidCell cell, FluidCell adjacent, bool loss)
		{
			if (flow == 0) return;
			remainingValue -= flow;
			cell.Diff -= flow;
			if (!loss || flow > 0.005f) // This makes fluids lose size as they move but makes the 
			{
				adjacent.Diff += flow;
			}
			adjacent.FluidId = cell.FluidId;
			adjacent.UpdateVisuals = true;
			UnsettleCell(adjacent);
		}

		public bool HorizontalFlowUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow, int distance)
		{
			FluidCell adjacent = GetFluidCell(cell.Position + GetVectorDirectionFromFlow(flowDirection) * distance);
			if (!CanFlowInto(cell, adjacent)) return false;
			
			flow = (remainingValue - adjacent.Liquid)/4;
			
			if (flow > MIN_FLOW)
				flow *= FLOW_SPEED;
			
			flow = Mathf.Max(flow, 0);
			float currentMax = Mathf.Min(MAX_FLOW, remainingValue);
			if (flow > currentMax)
				flow = currentMax;

			UpdateFlowValues(ref remainingValue, flow, cell, adjacent,true);
			return true;
		}

		public void RiseFluidUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow)
		{
			FluidCell adjacent = GetFluidCellInDirection(cell, flowDirection);
			if (!CanFlowInto(cell, adjacent)) return;
			
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
			Vector2Int chunkPosition = Global.getChunkFromCell(cellPosition);
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
