using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps;
using Chunks.Partitions;
using UnityEngine.Tilemaps;
using Items;
using System.Linq;
using Chunks;
using Chunks.Systems;
using Dimensions;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles.Fluid.Simulation;
using Random = UnityEngine.Random;

namespace Fluids {
    public class FluidWorldTileMap : AbstractIWorldTileMap<FluidTileItem>, ITileMapListener
    {
        private Tilemap unlitTileMap;
        public void Awake()
        {
            simulator = new FluidTileMapSimulator(this);
            itemRegistry = ItemRegistry.GetInstance();
        }
        
        

        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            gameObject.tag = "Fluid";
            GameObject unlitContainer = new GameObject("Unlit");
            unlitContainer.tag = "Fluid";
            unlitContainer.transform.SetParent(transform,false);
            unlitTileMap = unlitContainer.AddComponent<Tilemap>();
            unlitContainer.layer = LayerMask.NameToLayer("Fluid");
            TilemapRenderer tilemapRenderer = unlitContainer.AddComponent<TilemapRenderer>();
            tilemapRenderer.material = DimensionManager.Instance.MiscDimAssets.LitMaterial;
            var unlitCollider = unlitContainer.AddComponent<TilemapCollider2D>();
            unlitCollider.isTrigger = true;
            tilemapCollider.isTrigger = true;
            // why can't we just disable this unity. God forbid some poor soul manages to break this many blocks. RIP PC
            unlitCollider.maximumTileChangeCount=int.MaxValue; 
        }
        private ItemRegistry itemRegistry;

        private FluidTileMapSimulator simulator;
        public FluidTileMapSimulator Simulator => simulator;
        public const float MAX_FILL = 1f;
        public override bool HitTile(Vector2 position, bool dropItem)
        {
            return false;
            // Cannot hit fluid tiles
        }

        public override ItemObject GetItemObject(Vector2Int position)
        {
            // Isn't required
            return null;
        }

        public override bool BreakAndDropTile(Vector2Int position, bool dropItem)
        {
            return false;
        }

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            unlitTileMap.SetTile(new Vector3Int(x, y, 0), null);
        }

        public void AddChunk(ILoadedChunk loadedChunk)
        {
            FluidCell[][] fluidCells = new FluidCell[Global.CHUNK_SIZE][];
            for (int index = 0; index < Global.CHUNK_SIZE; index++)
            {
                fluidCells[index] = new FluidCell[Global.CHUNK_SIZE];
            }

            for (int px = 0; px < Global.PARTITIONS_PER_CHUNK; px++)
            {
                for (int py = 0; py < Global.PARTITIONS_PER_CHUNK; py++)
                {
                    IChunkPartition partition = loadedChunk.GetPartition(new Vector2Int(px, py));
                    partition.AddFluidDataToChunk(fluidCells);
                }
            }

            simulator.AddChunk(loadedChunk.GetPosition(), fluidCells);
        }

        public void RemoveChunk(Vector2Int position)
        {
            simulator.RemoveChunk(position);
        }

        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            return Global.getCellPositionFromWorld(position);
        }

        protected override void SetTile(int x, int y, FluidTileItem item)
        {
            if (ReferenceEquals(item,null)) {
                tilemap.SetTile(new Vector3Int(x,y,0),null);
                unlitTileMap.SetTile(new Vector3Int(x,y,0),null);
                return;
            }
            Vector2Int position = new Vector2Int(x,y);

            FluidCell fluidCell = simulator.GetFluidCell(position);

            DisplayTile(x, y, item, fluidCell.Liquid);
        }

        public void DisplayTile(int x, int y, FluidTileItem fluidTileItem, float fill)
        {
            bool lit = fluidTileItem.fluidOptions.Lit;
            Tilemap map = lit ? unlitTileMap : tilemap;
            Vector3Int vector3Int = new Vector3Int(x, y, 0);
            
            int tileIndex = (int)(FluidTileItem.FLUID_TILE_ARRAY_SIZE * fill);
            Tile tile = fluidTileItem.getTile(tileIndex);
            map.SetTile(vector3Int,tile);
            if (lit)
            {
                map.SetTileFlags(vector3Int,TileFlags.None);
                map.SetColor(vector3Int,Color.white*0.9f);
            }
        }
        
        public void DisplayTile(int x, int y, string id, float fill)
        {
            FluidTileItem fluidTileItem = itemRegistry.GetFluidTileItem(id);
            if (!fluidTileItem)
            {
                tilemap.SetTile(new Vector3Int(x,y,0),null);
                unlitTileMap.SetTile(new Vector3Int(x,y,0),null);
                return;
            }
            DisplayTile(x,y,fluidTileItem, fill);
        }
        
        public void DisplayTile(FluidCell fluidCell)
        {
            var fluidTileItem = itemRegistry.GetFluidTileItem(fluidCell.FluidId);
            if (!fluidTileItem)
            {
                tilemap.SetTile(new Vector3Int(fluidCell.Position.x,fluidCell.Position.y,0),null);
                unlitTileMap.SetTile(new Vector3Int(fluidCell.Position.x,fluidCell.Position.y,0),null);
                return;
            }
            DisplayTile(fluidCell.Position.x,fluidCell.Position.y,fluidTileItem,fluidCell.Liquid);
        }
        
        
        protected override void WriteTile(IChunkPartition partition, Vector2Int positionInPartition, FluidTileItem item)
        {
            PartitionFluidData partitionFluidData = partition.GetFluidData();
            partitionFluidData.ids[positionInPartition.x,positionInPartition.y] = item.id;
            partitionFluidData.fill[positionInPartition.x,positionInPartition.y] = MAX_FILL;
            Vector2Int cellPosition = partition.GetRealPosition() * Global.CHUNK_PARTITION_SIZE + positionInPartition;
            FluidCell fluidCell = new FluidCell(item.id,MAX_FILL,FluidFlowRestriction.NoRestriction,cellPosition,true);
            simulator.AddFluidCell(fluidCell);
        }

        public void TileUpdate(Vector2Int position)
        {
            ILoadedChunkSystem chunkSystem = closedChunkSystem;
            var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(position);
            FluidCell fluidCell = partition.GetFluidCell(positionInPartition, true);
            simulator.AddFluidCell(fluidCell);
            simulator.UnsettleNeighbors(position);
        }

        public void FixedUpdate()
        {
            simulator.Simulate();
            
            const int RANDOM_UNLIT_LIGHT_CHANCE = 10;
            int ran = Random.Range(0, RANDOM_UNLIT_LIGHT_CHANCE);
            if (ran != 0) return;
            
            // Calls this about once per second
            unlitTileMap.ResizeBounds();
            BoundsInt bounds = unlitTileMap.cellBounds;
            int size = bounds.size.x * bounds.size.y;
            const float HIGHEST_ODDS = 1024;
            //float random = UnityEngine.Random.value;
            //if (size / HIGHEST_ODDS < random) return;
            Vector3Int randomPosition = new Vector3Int(UnityEngine.Random.Range(bounds.min.x,bounds.max.x+1), UnityEngine.Random.Range(bounds.min.y,bounds.max.y+1),0);
            if (!unlitTileMap.HasTile(randomPosition)) return;
            Debug.Log("Unlit flash");
            StartCoroutine(FlashUnlitMap(randomPosition,4));

        }

        private IEnumerator FlashUnlitMap(Vector3Int origin, int size)
        {
            void Highlight(Vector3Int vector3Int)
            {
                unlitTileMap.SetTileFlags(vector3Int,TileFlags.None);
                Color current = unlitTileMap.GetColor(vector3Int);
                unlitTileMap.SetColor(vector3Int,Color.Lerp(Color.white,current,0.5f));
            }
            

            List<Color> colors = new List<Color>();
            List<Vector3Int> seen = new List<Vector3Int>();
            seen.Add(origin);
            int r = 0;
            var delay = new WaitForSeconds(0.25f);
            colors.Add(unlitTileMap.GetColor(origin));
            while (r < size)
            {
                void TryAddToSeen(Vector3Int vector3Int)
                {
                    if (seen.Contains(vector3Int)) return;
                    seen.Add(vector3Int);
                }
                int original = seen.Count;
                for (var index = 0; index < original; index++)
                {
                    var current = seen[index];
                    Highlight(current);
                    TryAddToSeen(current + Vector3Int.down);
                    TryAddToSeen(current + Vector3Int.left);
                    TryAddToSeen(current + Vector3Int.up);
                    TryAddToSeen(current + Vector3Int.right);
                }
                colors.Add(unlitTileMap.GetColor(origin));
                
                yield return delay;
                r++;
            }

            foreach (Color c in colors)
            {
                Debug.Log(c);
            }
            while (r >= 0)
            {
                int index = seen.Count-1;
                int colorIndex = r;
                while (colorIndex >= 0)
                {
                    int toColor = 4 * colorIndex;
                    Color color = colors[r-colorIndex];
                    Debug.Log(color + "," + toColor);
                    while (toColor > 0)
                    {
                        unlitTileMap.SetColor(seen[index],color);
                        toColor--;
                        index--;
                    }

                    if (colorIndex == 0)
                    {
                        unlitTileMap.SetColor(seen[index],color);
                    }
                    colorIndex--;
                }
                int removals = 4 * r;
                int count = seen.Count;
                for (int i = count-1; i >= count-removals; i--)
                {
                    seen.RemoveAt(i);
                }
               
                yield return delay;
                r--;
            }
        }
        
        
    }
}

