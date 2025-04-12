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
using Robot.Tool.Instances.Gun;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles.Fluid.Simulation;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Fluids {
    public class FluidWorldTileMap : AbstractIWorldTileMap<FluidTileItem>, ITileMapListener
    {
        private class FluidParticles
        {
            public ParticleSystem Splash;
            public ParticleSystem Standard;

            public FluidParticles(ParticleSystem splashPrefab, ParticleSystem baseSystemPrefab, Transform container, Material material)
            {
                ParticleSystem InstanatiateSystem(ParticleSystem prefab)
                {
                    var system = Instantiate(prefab, container, false);
                    var renderer = system.GetComponent<ParticleSystemRenderer>();
                    if (material) renderer.material = material;
                    return system;
                }
                Splash = InstanatiateSystem(splashPrefab);
                Standard = InstanatiateSystem(baseSystemPrefab);
            }
        }
        private Tilemap unlitTileMap;
        private TilemapCollider2D unlitCollider2D;
        private TilemapCollider2D mapCollider2D;
        private int flashCounter = 0;
        private int ticksToTryFlash = 25;

        private FluidParticles litFluidParticles;
        private FluidParticles unlitFluidParticles;
        
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
            MiscDimAssets miscDimAssets = DimensionManager.Instance.MiscDimAssets;
            tilemapRenderer.material = miscDimAssets.UnlitMaterial;
            litFluidParticles = new FluidParticles(miscDimAssets.SplashParticlePrefab, miscDimAssets.FluidParticlePrefab, transform, null);
            unlitFluidParticles = new FluidParticles(miscDimAssets.SplashParticlePrefab, miscDimAssets.FluidParticlePrefab, transform, miscDimAssets.UnlitMaterial);
            
            unlitCollider2D = unlitContainer.AddComponent<TilemapCollider2D>();
            mapCollider2D = tilemap.GetComponent<TilemapCollider2D>();
            unlitCollider2D.isTrigger = true;
            tilemapCollider.isTrigger = true;
            simulator = new FluidTileMapSimulator(this, closedChunkSystem.GetTileMap(TileMapType.Object) as WorldTileGridMap,closedChunkSystem.GetTileMap(TileMapType.Block) as WorldTileGridMap);
            
            // why can't we just disable this unity. God forbid some poor soul manages to break this many blocks. RIP PC. Isaac -2025 'yep'
            unlitCollider2D.maximumTileChangeCount=int.MaxValue; 
        }

        private FluidTileMapSimulator simulator;
        public FluidTileMapSimulator Simulator => simulator;
        const int FLOW_ALL = 15;
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

        public FluidTileItem GetFluidTile(Vector2Int position)
        {
            return simulator.GetFluidCell(position)?.FluidTileItem;
        }

        public override bool BreakAndDropTile(Vector2Int position, bool dropItem)
        {
            BreakTile(position);
            return true;
        }

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            unlitTileMap.SetTile(new Vector3Int(x, y, 0), null);
        }
        public FluidTileItem GetFluidItem(Vector2 worldPosition)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            FluidCell fluidCell = simulator.GetFluidCell((Vector2Int)cellPosition);
            return fluidCell?.FluidTileItem;
        }
        public float GetFill(Vector2 worldPosition)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            FluidCell fluidCell = simulator.GetFluidCell((Vector2Int)cellPosition);
            if (fluidCell == null || !fluidCell.FluidTileItem) return 0;
            return fluidCell.Liquid;
        }
        public void Disrupt(Vector2 worldPosition, Vector2Int cellPosition, FluidTileItem fluidTileItem)
        {
            if (!fluidTileItem) return;
            FluidParticles particles = GetFluidParticles(fluidTileItem.fluidOptions.Lit);
            
            if (particles.Splash.isPlaying) return;
            simulator.DisruptSurface(cellPosition);
            if (!fluidTileItem) return;
            PlayParticles(particles.Splash, worldPosition,fluidTileItem);
            
        }

        private FluidParticles GetFluidParticles(bool lit)
        {
            // Kind of confusing cause this is reversed. Probably when I made fluid tile items, I imagined lit as being lit all the time, but
            // in reality lit particles are ones that are effected by light.
            return !lit ? litFluidParticles : unlitFluidParticles;
        }
        void PlayParticles(ParticleSystem particles, Vector2 position, FluidTileItem fluidTileItem)
        {
            particles.transform.position = position;
            var mainModule = particles.main;
            mainModule.startColor = fluidTileItem.fluidOptions.ParticleColor;
            particles.Play();
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

        public override void BreakTile(Vector2Int position)
        {
            Vector3Int vector3Int = new Vector3Int(position.x, position.y, 0);
            tilemap.SetTile(vector3Int,null);
            unlitTileMap.SetTile(vector3Int,null);
        }

        public void RemoveChunk(Vector2Int position)
        {
            simulator.RemoveChunk(position);
        }

        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            return Global.GetCellPositionFromWorld(position);
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
            if (!fluidTileItem) return;
            bool lit = fluidTileItem.fluidOptions.Lit;
            Tilemap map = lit ? unlitTileMap : tilemap;
            Vector3Int vector3Int = new Vector3Int(x, y, 0);
            
            int tileIndex = (int)(FluidTileItem.FLUID_TILE_ARRAY_SIZE * fill);
            Tile tile = fluidTileItem.getTile(tileIndex);
            map.SetTile(vector3Int,tile);
            if (!lit) return;
            if (map.GetTileFlags(vector3Int) == TileFlags.None) return;
            map.SetTileFlags(vector3Int,TileFlags.None);
            map.SetColor(vector3Int,Color.white*0.9f);
        }
        
        
        public void DisplayTile(FluidCell fluidCell)
        {
            var fluidTileItem = fluidCell.FluidTileItem;
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
            partitionFluidData.ids[positionInPartition.x,positionInPartition.y] = item?.id;
            float fill = item ? MAX_FILL : 0;
            partitionFluidData.fill[positionInPartition.x,positionInPartition.y] = fill;
            Vector2Int cellPosition = partition.GetRealPosition() * Global.CHUNK_PARTITION_SIZE + positionInPartition;
            
            FluidCell fluidCell = new FluidCell(item,fill,FLOW_ALL,cellPosition,true);
            simulator.AddFluidCell(fluidCell,true);
        }

        public override bool HasTile(Vector3Int vector3Int)
        {
            return mTileMap.GetTile(vector3Int) || unlitTileMap.HasTile(vector3Int);
        }

        public void TileUpdate(Vector2Int position)
        {
            ILoadedChunkSystem chunkSystem = closedChunkSystem;
            var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(position);
            TileItem tileItem = partition.GetTileItem(positionInPartition,TileMapLayer.Base);
            if (tileItem) // Place
            {
                if (tileItem.tileType == TileType.Block)
                {
                    simulator.RemoveFluidCell(position);
                }

                if (tileItem.tileOptions.fluidBreakable)
                {
                    FluidCell current = simulator.GetFluidCell(position);
                    if (current != null && current.Liquid > 0.05f)
                    {
                        var tileGridMap = closedChunkSystem.GetTileMap(tileItem.tileType.toTileMapType()) as WorldTileGridMap;
                        tileGridMap?.BreakAndDropTile(position,true);
                    }
                    
                }
            }
            else // Break
            {
                FluidCell current = simulator.GetFluidCell(position);
                if (current == null)
                {
                    FluidCell fluidCell = new FluidCell(null, 0, FLOW_ALL, position,true);
                    simulator.AddFluidCell(fluidCell,true);
                }
                else
                {
                    current.FlowBitMap = FLOW_ALL;
                }
            }
            
            simulator.UnsettleNeighbors(position);
        }

        public void FixedUpdate()
        {
            simulator.Simulate();
            RandomlyFlashUnlitMap();
            DisplayRandomParticles(unlitTileMap,unlitCollider2D);
            DisplayRandomParticles(tilemap,mapCollider2D);
        }

        private void RandomlyFlashUnlitMap()
        {
            flashCounter++;
            if (flashCounter < ticksToTryFlash) return;
            flashCounter = 0;
            Bounds bounds = unlitCollider2D.bounds;
            
            // Large pool of lava in large camera view has ~1000 tiles
            const float CHANCE = 1024;
            TileMapPositionInfo? randomPosition = GetRandomCellPosition(ref bounds,CHANCE);
            if (!randomPosition.HasValue) return;
            Vector3Int cellPosition = randomPosition.Value.CellPosition;
            if (!unlitTileMap.HasTile(cellPosition) || unlitTileMap.GetColor(cellPosition) != Color.white * 0.9f) return;
           
            int flashSize = UnityEngine.Random.Range(6, 10);
            StartCoroutine(FlashUnlitMap(cellPosition,flashSize));
        }

        private TileMapPositionInfo? GetRandomCellPosition(ref Bounds bounds, float chance)
        {
            float size = bounds.size.x * bounds.size.y;
            if (size == 0) return null;
            
            float random = UnityEngine.Random.value;
            if (size / chance < random) return null;
            
            Vector2 randomWorldPosition = new Vector2(UnityEngine.Random.Range(bounds.min.x,bounds.max.x+1),UnityEngine.Random.Range(bounds.min.y,bounds.max.y+1));
            Vector3Int randomCellPosition = unlitTileMap.WorldToCell(randomWorldPosition);
            randomCellPosition.z = 0;
            return new TileMapPositionInfo
            {
                CellPosition = randomCellPosition,
                WorldPosition = randomWorldPosition
            };

        }
        
        

        private void DisplayRandomParticles(Tilemap map, Collider2D mapCollider)
        {
            Bounds bounds = mapCollider.bounds;
            
            TileMapPositionInfo? nullableRandomPosition = GetRandomCellPosition(ref bounds,256);
            if (!nullableRandomPosition.HasValue) return;
            
            TileMapPositionInfo tileMapPositionInfo = nullableRandomPosition.Value;
            Vector3Int randomCellPosition = tileMapPositionInfo.CellPosition;
            if (!map.HasTile(randomCellPosition)) return;
            
            FluidCell fluidCell = simulator.GetFluidCell((Vector2Int)randomCellPosition);
            if (fluidCell == null || fluidCell.Liquid < 0.95f || !fluidCell.FluidTileItem) return;

            FluidParticles particles = GetFluidParticles(fluidCell.FluidTileItem.fluidOptions.Lit);
            PlayParticles(particles.Standard, tileMapPositionInfo.WorldPosition, fluidCell.FluidTileItem);
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
            var delay = new WaitForSeconds(0.1f);
            colors.Add(unlitTileMap.GetColor(origin));

            void IncreaseSize()
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
            }

            void DecreaseSize()
            {
                int index = seen.Count-1;
                int colorIndex = r;
                while (colorIndex >= 0)
                {
                    int toColor = 4 * colorIndex;
                    Color color = colors[r-colorIndex];
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
            }

            const int MIN_SIZE = 3;
            const float INCREASE_ODDS = 0.6f;
            while (r < size)
            {
                if (r > MIN_SIZE)
                {
                    float ran = UnityEngine.Random.value;
                    if (ran < INCREASE_ODDS)
                    {
                        IncreaseSize();
                        r++;
                    }
                    else
                    {
                        DecreaseSize();
                        r--;
                    }
                }
                else
                {
                    IncreaseSize();
                    r++;
                }
                yield return delay;
            }
            
            while (r >= 0)
            {
                DecreaseSize();
                yield return delay;
                r--;
            }
        }
        
        private struct TileMapPositionInfo {
            public Vector3Int CellPosition;
            public Vector2 WorldPosition;
            
        }
    }
}

