using System;
using System.Collections.Generic;
using Fluids;
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
		public bool Settled;
		public FluidFlowRestriction FlowRestriction;
		public Vector2Int Position;
		public float Diff;
		public int SettleCount;

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
	
    public class FluidTileMapSimulator
    {
	    public FluidTileMapSimulator(FluidWorldTileMap fluidWorldTileMap)
	    {
		    this.fluidWorldTileMap = fluidWorldTileMap;
	    }

	    // Implementation that might be better than storing settling
	    private uint ticks;
	    private Dictionary<uint, HashSet<Vector2Int>> tickFluidUpdates = new Dictionary<uint, HashSet<Vector2Int>>(); 
	    
	    private Dictionary<Vector2Int, FluidCell[][]> chunkCellArrayDict = new(); 
	    const float MAX_FILL = 1.0f;
	    const float MIN_FILL = 0.005f;

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
        }
        
		public void Simulate() {
			foreach (var chunkFluidCellCollection in chunkCellArrayDict.Values)
			{
				foreach (FluidCell[] fluidCells in chunkFluidCellCollection)
				{
					foreach (FluidCell cell in fluidCells)
					{
						cell.Diff = 0;
					}
				}
			}

			foreach (var chunkFluidCellCollection in chunkCellArrayDict.Values)
			{
				foreach (FluidCell[] fluidCells in chunkFluidCellCollection)
				{
					foreach (FluidCell cell in fluidCells)
					{
						if (cell.FlowRestriction == FluidFlowRestriction.BlockFluids) {
							cell.Liquid = 0;
							continue;
						}
						if (cell.Liquid == 0)
							continue;
						if (cell.Settled) 
							continue;
						if (cell.Liquid < MIN_FILL) {
							cell.Liquid = 0;
							continue;
						}

						// Keep track of how much liquid this cell started off with
						float startValue = cell.Liquid;
						float remainingValue = cell.Liquid;
						float flow = 0;
						
						FallFlowUpdate(cell,FluidFlowDirection.Down,ref remainingValue, ref flow); // TODO falling up 
					
						if (remainingValue < MIN_FILL) {
							cell.Diff -= remainingValue;
							continue;
						}
						
						// Flow to left cell
						HorizontalFlowUpdate(cell,FluidFlowDirection.Left,ref remainingValue, ref flow);
					
						if (remainingValue < MIN_FILL) {
							cell.Diff -= remainingValue;
							continue;
						}
					
						HorizontalFlowUpdate(cell,FluidFlowDirection.Right,ref remainingValue, ref flow);
					
						// Check to ensure we still have liquid in this cell
						if (remainingValue < MIN_FILL) {
							cell.Diff -= remainingValue;
							continue;
						}
					
						RiseFluidUpdate(cell,FluidFlowDirection.Up,ref remainingValue, ref flow);
						// Check to ensure we still have liquid in this cell
						if (remainingValue < MIN_FILL) {
							cell.Diff -= remainingValue;
							continue;
						}
						
						// Check if cell is settled
						if (startValue == remainingValue) {
							
							cell.SettleCount++;
							if (cell.SettleCount >= 10) {
								cell.Settled = true;
							}
						} else
						{
							UnsettleNeighbors(cell);
						}
					}
				}
				
			}

			foreach (var chunkFluidCellCollection in chunkCellArrayDict.Values)
			{
				foreach (FluidCell[] fluidCells in chunkFluidCellCollection)
				{
					foreach (FluidCell cell in fluidCells)
					{
						cell.Liquid += cell.Diff;

						if (cell.Liquid > MIN_FILL)
						{
							fluidWorldTileMap.DisplayTile(cell);
							continue;
						}
						
						cell.Liquid = 0;
						cell.FluidId = null;
						cell.Settled = false;
						fluidWorldTileMap.DisplayTile(cell);
					}
				}
			}
		}

		public void FallFlowUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow)
		{
			FluidCell adjacent = GetFluidCellInDirection(cell, flowDirection);
			if (!CanFlowInto(cell, adjacent)) return;
			
			flow = CalculateVerticalFlowValue(cell.Liquid, adjacent) - adjacent.Liquid;
			if (adjacent.Liquid > 0 && flow > MIN_FLOW)
				flow *= FLOW_SPEED; 
			
			flow = Mathf.Max (flow, 0);
			if (flow > Mathf.Min(MAX_FLOW, cell.Liquid)) 
				flow = Mathf.Min(MAX_FLOW, cell.Liquid);
			
			UpdateFlowValues(ref remainingValue, flow, cell, adjacent);
		}

		public void UnsettleNeighbors(FluidCell cell)
		{
			void UnsettleCell(FluidCell adjacent)
			{
				if (adjacent == null) return;
				adjacent.Settled = false;
			}
			Vector2Int cellPosition = cell.Position;

			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.up));
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.down));
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.left));
			UnsettleCell(GetFluidCell(cellPosition + Vector2Int.right));
		}

		public bool CanFlowInto(FluidCell fluidCell, FluidCell adj)
		{
			return adj != null && adj.FlowRestriction != FluidFlowRestriction.BlockFluids && (adj.FluidId == null || string.Equals(fluidCell.FluidId, adj.FluidId));
		}
		public void UpdateFlowValues(ref float remainingValue, float flow, FluidCell cell, FluidCell adjacent)
		{
			if (flow == 0) return;
			remainingValue -= flow;
			cell.Diff -= flow;
			adjacent.Diff += flow;
			adjacent.Settled = false;
			adjacent.FluidId = cell.FluidId;
		}

		public void HorizontalFlowUpdate(FluidCell cell, FluidFlowDirection flowDirection, ref float remainingValue, ref float flow)
		{
			FluidCell adjacent = GetFluidCellInDirection(cell, flowDirection);
			if (!CanFlowInto(cell, adjacent)) return;
			
			flow = (remainingValue - adjacent.Liquid)/4;
			
			if (flow > MIN_FLOW)
				flow *= FLOW_SPEED;
			
			flow = Mathf.Max(flow, 0);
			float currentMax = Mathf.Min(MAX_FLOW, remainingValue);
			if (flow > currentMax)
				flow = currentMax;

			UpdateFlowValues(ref remainingValue, flow, cell, adjacent);
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
			
			UpdateFlowValues(ref remainingValue, flow, cell, adjacent);
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
