using System;
using System.Collections.Generic;
using Chunks.Systems;
using Conduit.Conduit;
using Conduits;
using Dimensions;
using Item.Slot;
using Items;
using Newtonsoft.Json;
using Player;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.Instances.VeinMine;
using Robot.Upgrades.LoadOut;
using TileMaps;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles;
using Tiles.Indicators;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = Unity.Mathematics.Random;


namespace Robot.Tool.Instances
{
    public class LaserDrill : RobotToolInstance<LaserDrillData, RobotDrillObject>, IAcceleratedClickHandler, IDestructiveTool
    {
        private LineRenderer lineRenderer;
        private ParticleSystem particleSystem;
        private bool hitting;
        public LaserDrill(LaserDrillData toolData, RobotDrillObject robotObject, RobotStatLoadOutCollection loadOut, PlayerScript playerScript) : base(toolData, robotObject, loadOut, playerScript)
        {
            particleSystem = GameObject.Instantiate(robotObject.ParticleEmitterPrefab, playerScript.transform);
        }
        
        public override Sprite GetPrimaryModeSprite()
        {
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                    return robotObject.BaseLayerSprite;
                case TileMapLayer.Background:
                    return robotObject.BackgroundLayerSprite;
                default:
                    return null;
            }
        }

        public override void BeginClickHold(Vector2 mousePosition)
        {
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            lineRenderer = GameObject.Instantiate(robotObject.LineRendererPrefab,playerTransform);
            UpdateLineRenderer(mousePosition);
        }

        public override void TerminateClickHold()
        {
            GameObject.Destroy(lineRenderer.gameObject);
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey != MouseButtonKey.Left) return;
            UpdateLineRenderer(mousePosition);
            particleSystem.transform.position = mousePosition;

            if (toolData.Layer == TileMapLayer.Base)
            {
                hitting = MouseUtils.RaycastObject(mousePosition, toolData.Layer.toRaycastLayers());
                if (!hitting)
                {
                    return;
                }
                
            }
            else
            {
                // This works I think ?
                hitting = true;
            }

            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            WorldTileGridMap worldTileGridMap = GetWorldTileGridMap(closedChunkSystem);
            
            if (!worldTileGridMap) return;
            
            bool drop = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Item_Magnet) == 0;
            
            int drillPower = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Tier);
            float veinMineUpgrades = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)RobotDrillUpgrade.VeinMine);
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMineUpgrades);
            
            int multiBreak = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.MultiBreak);
            TileItem tileItem = worldTileGridMap.GetTileItem(mousePosition);
            
            
            
            Color? particleColor = GetParticleSystemColor(tileItem);
            if (particleColor.HasValue)
            {
                ParticleSystem.MainModule particleSystemMain = particleSystem.main;
                particleSystemMain.startColor = particleColor.Value;
            }
            
            if (multiBreak == 0)
            {
                bool broken = MouseUtils.HitTileLayer(toolData.Layer, mousePosition, drop, RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier),true);
                if (broken)
                {
                    if (!drop && !DevMode.Instance.instantBreak)
                    {
                        List<ItemSlot> itemDrops = ItemSlotUtils.GetTileItemDrop(tileItem);
                        playerScript.PlayerInventory.GiveItems(itemDrops);
                    }
                    TryVeinMine(worldTileGridMap, tileItem, drop, mousePosition, veinMinePower, drillPower);
                    
                }
                return;
            }

            PlayerInventory playerInventory = playerScript.PlayerInventory;
            bool anyBroken = false;
            for (int x = -multiBreak; x <= multiBreak; x++)
            {
                for (int y = -multiBreak; y <= multiBreak; y++)
                {
                    Vector2 position = mousePosition + Global.TILE_SIZE * new Vector2(x, y);
                    TileItem multiHitTileItem = worldTileGridMap.GetTileItem(position);
                    bool broken = MouseUtils.HitTileLayer(toolData.Layer, position, drop,RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier),false);
                    if (broken) anyBroken = true;
                    if (broken && !drop)
                    {
                        playerInventory.GiveItems(ItemSlotUtils.GetTileItemDrop(multiHitTileItem));
                    }
                }
            }

            if (anyBroken)
            {
                playerScript.PlayerMouse.UpdateOnToolChange();
            }
        }

        private Color? GetParticleSystemColor(TileItem tileItem)
        {
            if (!tileItem) return null;
            TileParticleOptions gradient = tileItem.tileOptions?.ParticleGradient;

            if (gradient == null) return null;
            int source = UnityEngine.Random.Range(0, 2);
            const int OVERLAY_SOURCE = 0;
            const int GRADIENT_SOURCE = 1;
            switch (source)
            {
                case OVERLAY_SOURCE:
                    if (tileItem.tileOptions.Overlay)
                    {
                        return tileItem.tileOptions.Overlay.GetColor();
                    }
                    break;
                case GRADIENT_SOURCE:
                    float ran = UnityEngine.Random.Range(0f, 1f);
                    Color color = Color.Lerp(gradient.FirstGradientColor, gradient.SecondGradientColor, ran);
                    return color;
            }
            return null;
        }

        private WorldTileGridMap GetWorldTileGridMap(ClosedChunkSystem closedChunkSystem)
        {
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                    return closedChunkSystem.GetTileMap(TileMapType.Block) as WorldTileGridMap;
                case TileMapLayer.Background:
                    return closedChunkSystem.GetTileMap(TileMapType.Background) as WorldTileGridMap;
                
            }

            return null;
        }

        private bool TryVeinMine(WorldTileGridMap worldTileGridMap, TileItem initialItem, bool drop, Vector2 mousePosition, int veinMinePower, int drillPower)
        {
            if (veinMinePower <= 1) return false;
            Vector2Int cellPosition = Global.getCellPositionFromWorld(mousePosition);
            BlockVeinMineEvent blockVeinMineEvent = GetVeinMineEvent(worldTileGridMap, drop, initialItem, drillPower) as BlockVeinMineEvent;
            int? broken = blockVeinMineEvent?.Execute(cellPosition, veinMinePower);
            if (broken < 1) return false;
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            if (!drop)
            {
                List<ItemSlot> itemDrops = blockVeinMineEvent?.GetCollectedItems();
                playerScript.PlayerInventory.GiveItems(itemDrops);
            }
            playerScript.TileViewers.TileBreakHighlighter.Clear();

            
            return true;
        }

        private IVeinMineEvent GetVeinMineEvent(WorldTileGridMap worldTileGridMap, bool drop, TileItem initialTile, int drillPower)
        {
            TileMapLayer tileMapLayer = toolData.Layer;
            switch (tileMapLayer)
            {
                case TileMapLayer.Base:
                    int hardness = initialTile.tileOptions.hardness;
                    return new BlockVeinMineEvent(worldTileGridMap, drop, drillPower, hardness);
                case TileMapLayer.Background:
                    return new BackGroundVeinMineEvent(worldTileGridMap, drop);
                default:
                    return null;
            }
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            if (mouseButtonKey != MouseButtonKey.Left)
            {
                hitting = false;
                return false;
            }
            Debug.Log(hitting);
           
           
            if (toolData.Layer == TileMapLayer.Base)
            {
                if (particleSystem.isStopped && hitting)
                {
                    particleSystem.Play();
                }

                if (particleSystem.isPlaying && !hitting)
                {
                    particleSystem.Stop();
                }
            }
            else
            {
                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop();
                }
            }
            
            bool pass = time >= toolData.HitRate;
            if (!pass)
            {
                UpdateLineRenderer(mousePosition);
                return false;
            }
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            toolData.Layer = toolData.Layer == TileMapLayer.Base ? TileMapLayer.Background : TileMapLayer.Base;
        }

        public override string GetModeName()
        {
            return toolData?.Layer.ToString();
        }

        public override void Preview(Vector2Int cellPosition)
        {
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                    PreviewBaseLayer(cellPosition);
                    break;
                case TileMapLayer.Background: // TODO
                    break;
            }
        }

        private void PreviewBaseLayer(Vector2Int cellPosition)
        {
            int drillPower = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Tier);
            float veinMineUpgrades = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)RobotDrillUpgrade.VeinMine);
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMineUpgrades);
            
            
            int multiBreak = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.MultiBreak);
            TileBreakHighlighter tileBreakHighlighter = playerScript.TileViewers.TileBreakHighlighter;
            Dictionary<Vector2Int, OutlineTileMapCellData> outlineDict = GetOutlineCellData(cellPosition,drillPower,multiBreak,veinMinePower);
            if (outlineDict == null)
            {
                tileBreakHighlighter.Clear();
                return;
            }
            
            tileBreakHighlighter.Display(outlineDict);
        }

        private Dictionary<Vector2Int, OutlineTileMapCellData> GetOutlineCellData(Vector2Int cellPosition, int drillPower, int multiBreak, int veinMinePower)
        {
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            
            
            
            if (multiBreak == 0)
            {
                if (veinMinePower < 2) return null;
                WorldTileGridMap worldTileGridMap = GetWorldTileGridMap(closedChunkSystem);
                IOutlineTileGridMap outlineTileGridMap = worldTileGridMap as IOutlineTileGridMap;
                if (outlineTileGridMap == null) return null;
                TileItem tileItem = worldTileGridMap.getTileItem(cellPosition);
          
                if (!tileItem) return null;
         
                IVeinMineEvent veinMineEvent = GetVeinMineEvent(worldTileGridMap, false, tileItem, drillPower);
                HashSet<Vector2Int> brokenPositions = veinMineEvent.Preview(cellPosition, veinMinePower);
                if (brokenPositions.Count == 1) return null;
                Dictionary<Vector2Int, OutlineTileMapCellData> veinMineTiles = new Dictionary<Vector2Int, OutlineTileMapCellData>();
                foreach (Vector2Int position in brokenPositions)
                {
                    veinMineTiles[position] = outlineTileGridMap.GetOutlineCellData(new Vector3Int(position.x, position.y,0));
                }
                return veinMineTiles;
            }
            
            List<IWorldTileMap> worldTileGridMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Block),
                closedChunkSystem.GetTileMap(TileMapType.Object)
            };

            
            Dictionary<Vector2Int, OutlineTileMapCellData> tiles = new Dictionary<Vector2Int, OutlineTileMapCellData>();
            for (int x = -multiBreak; x <= multiBreak; x++)
            {
                for (int y = -multiBreak; y <= multiBreak; y++)
                {
                    Vector2Int breakPosition = cellPosition + new Vector2Int(x, y);
                    foreach (IWorldTileMap tileGridMap in worldTileGridMaps)
                    {
                        if (!tileGridMap.hasTile(breakPosition)) continue;
                        
                        if (tileGridMap is IOutlineTileGridMap outlineTileGridMap)
                        {
                            tiles[breakPosition] = outlineTileGridMap.GetOutlineCellData(new Vector3Int(breakPosition.x, breakPosition.y, 0));
                        }
                        else
                        {
                            tiles[breakPosition] = new OutlineTileMapCellData(tileGridMap.GetTilemap().GetTile(new Vector3Int(breakPosition.x, breakPosition.y, 0)),null,Quaternion.identity);
                        }
                         
                    }
                    
                }
            }

            return tiles;
        }

        private void UpdateLineRenderer(Vector2 mousePosition)
        {
            Vector2 dif =  mousePosition - (Vector2) PlayerManager.Instance.GetPlayer().transform.position;
            lineRenderer.SetPositions(new Vector3[] { Vector3.up/2f, dif });
        }

        public float GetSpeedMultiplier()
        {
            return 1 + RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)RobotDrillUpgrade.Speed);
        }
    }

    public class LaserDrillData : RobotToolData
    {
        public TileMapLayer Layer;
        public float HitRate;
        public int HitDamage;
        public LaserDrillData(TileMapLayer layer, float hitRate, int hitDamage)
        {
            Layer = layer;
            HitRate = hitRate;
            HitDamage = hitDamage;
        }
    }
}
