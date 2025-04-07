using System;
using System.Collections.Generic;
using Chunks.Systems;
using Conduit.Conduit;
using Conduits;
using Dimensions;
using Fluids;
using Item.Slot;
using Items;
using Newtonsoft.Json;
using Player;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using Robot.Tool.Instances.Drill;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.Instances.VeinMine;
using Robot.Upgrades.LoadOut;
using TileMaps;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles;
using Tiles.Indicators;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = Unity.Mathematics.Random;


namespace Robot.Tool.Instances
{
    public class LaserDrill : RobotToolInstance<LaserDrillData, RobotDrillObject>, IAcceleratedClickHandler, IDestructiveTool, IAutoSelectTool
    {
        private RobotToolLaserManager laserManager;
        private ParticleSystem particleSystem;
        private LaserDrillAudioController audioController;
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
                case TileMapLayer.Fluid:
                    return robotObject.FluidLayerSprite;
                default:
                    return null;
            }
        }

        public override void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey == MouseButtonKey.Right) return;
            
            laserManager = new RobotToolLaserManager(GameObject.Instantiate(robotObject.LineRendererPrefab, playerScript.transform));
            laserManager.UpdateLineRenderer(mousePosition,GetColor());
            audioController = GameObject.Instantiate(base.robotObject.AudioControllerPrefab, playerScript.transform);
        }

        public override void TerminateClickHold(MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey == MouseButtonKey.Right) return;
            playerScript.TileViewers.TileBreakHighlighter.Clear();
            laserManager?.Terminate();
            if (audioController)
            {
                GameObject.Destroy(audioController.gameObject);
                audioController = null;
            }
            
        }

        private void HitFluid(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey != MouseButtonKey.Left) return;
            laserManager.UpdateLineRenderer(mousePosition,GetColor());
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            FluidWorldTileMap fluidWorldTileMap = closedChunkSystem.GetFluidTileMap();
            float fill = fluidWorldTileMap.GetFill(mousePosition);
            FluidTileItem fluidTileItem = fluidWorldTileMap.GetFluidItem(mousePosition);
            playerScript.PlayerInventory.GiveFluid(fluidTileItem,fill);
            fluidWorldTileMap.DeleteTile(mousePosition);
        }
        private void HitTile(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey != MouseButtonKey.Left) return;
            laserManager.UpdateLineRenderer(mousePosition,GetColor());
            particleSystem.transform.position = mousePosition;

            if (toolData.Layer == TileMapLayer.Base)
            {
                hitting = MouseUtils.RaycastObject(mousePosition, toolData.Layer.toRaycastLayers());
                if (!hitting) return;
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
            
            audioController?.PlayAudioClip(tileItem?.tileOptions?.AudioType ?? TileAudioType.None);
            
            SetParticleSystemColor(tileItem);
            const float minPercent = 0.1f;
            if (!playerRobot.TryConsumeEnergy(RobotDrillUpgradeInfo.COST_PER_HIT,minPercent)) return;
            bool broken = MouseUtils.HitTileLayer(toolData.Layer, mousePosition, drop, RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier),true);
            if (multiBreak == 0)
            {
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
            

            bool Break(int x, int y)
            {
                Vector2 position = mousePosition + Global.TILE_SIZE * new Vector2(x, y);
                TileItem multiHitTileItem = worldTileGridMap.GetTileItem(position);
                if (!playerRobot.TryConsumeEnergy(RobotDrillUpgradeInfo.COST_PER_HIT,minPercent)) return false;
                bool broken = MouseUtils.HitTileLayer(
                    toolData.Layer, 
                    position, 
                    drop,
                    RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Tier), 
                    false
                );
                if (broken) anyBroken = true;
                if (broken && !drop)
                {
                    playerInventory.GiveItems(ItemSlotUtils.GetTileItemDrop(multiHitTileItem));
                }
                return true;
            }
            
            int r = 0;
            while (r <= multiBreak)
            {
                for (int x = -r; x < r; x++)
                {
                    if (!Break(x, r)) break;
                }
                for (int y = r; y > -r; y--)
                {
                    if (!Break(r, y)) break;
                }
                for (int x = r; x > -r; x--)
                {
                    if (!Break(x, -r)) break;
                }
                for (int y = -r; y < r; y++)
                {
                    if (!Break(-r, y)) break;
                }
                r++;
            }

            if (anyBroken)
            {
                playerScript.PlayerMouse.ClearToolPreview();
            }
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                case TileMapLayer.Background:
                    HitTile(mousePosition,mouseButtonKey);
                    break;
                case TileMapLayer.Fluid:
                    HitFluid(mousePosition,mouseButtonKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetParticleSystemColor(TileItem tileItem)
        {
            if (!tileItem) return;
            TileParticleOptions gradient = tileItem.tileOptions?.ParticleGradient;
            ParticleSystem.MainModule particleSystemMain = particleSystem.main;
            var minMaxColor = particleSystemMain.startColor;
            if (gradient == null) return;
            float ran = UnityEngine.Random.Range(0f, 1f);
            Color color = Color.Lerp(gradient.FirstGradientColor, gradient.SecondGradientColor, ran);
            minMaxColor.colorMax = color;
            minMaxColor.colorMin = tileItem.tileOptions.Overlay?.GetColor() ?? color;
            particleSystemMain.startColor = minMaxColor;
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

            bool EnergyCostFunction()
            {
                return playerRobot.TryConsumeEnergy(RobotDrillUpgradeInfo.COST_PER_HIT, 0.1f);
            }
            switch (tileMapLayer)
            {
                case TileMapLayer.Base:
                    int hardness = initialTile.tileOptions.hardness;
                    return new BlockVeinMineEvent(worldTileGridMap, drop, EnergyCostFunction, drillPower, hardness);
                case TileMapLayer.Background:
                    return new BackGroundVeinMineEvent(worldTileGridMap, drop, EnergyCostFunction);
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

            UpdateParticles();
            
            
            bool pass = time >= toolData.HitRate;
            if (!pass)
            {
                laserManager.UpdateLineRenderer(mousePosition,GetColor());
                return false;
            }
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        private void UpdateParticles()
        {
            if (!hitting || toolData.Layer != TileMapLayer.Base)
            {
                if (!particleSystem.isPlaying) return;
                particleSystem.Stop();
                return;
            }
            
            if (!particleSystem.isPlaying)
            {
                particleSystem.Play();
                return;
            }
            
            ParticleSystem.MainModule particleSystemMain = particleSystem.main;
            if (!Mathf.Approximately(particleSystemMain.duration, toolData.HitRate)) return;
            particleSystem.Stop();
            particleSystemMain.duration = toolData.HitRate; 
            particleSystem.Play();


        }

        public Color GetColor()
        {
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                    return new Color(145f / 255, 100f / 255, 1f, 1f);
                case TileMapLayer.Background:
                    return new Color(46f / 255, 29f / 255, 89f/255, 1f);
                case TileMapLayer.Fluid:
                    return new Color(0f / 255, 29f / 255, 89f/255, 1f);
            }
            return Color.white;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                    toolData.Layer = moveDirection == MoveDirection.Left ? TileMapLayer.Background : TileMapLayer.Fluid; 
                    break;
                case TileMapLayer.Background:
                    toolData.Layer = moveDirection == MoveDirection.Left ? TileMapLayer.Fluid : TileMapLayer.Base; 
                    break;
                case TileMapLayer.Fluid:
                    toolData.Layer = moveDirection == MoveDirection.Left ? TileMapLayer.Base : TileMapLayer.Background; 
                    break;
            }
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
                        if (!tileGridMap.HasTile(breakPosition)) continue;
                        Vector3Int vector3Int = new Vector3Int(breakPosition.x,breakPosition.y,0);
                        if (tileGridMap is IOutlineTileGridMap outlineTileGridMap)
                        {
                            tiles[breakPosition] = outlineTileGridMap.GetOutlineCellData(vector3Int);
                        }
                        else
                        {
                            tiles[breakPosition] = tileGridMap.FormatMainTileMapOutlineData(vector3Int);
                        }
                         
                    }
                    
                }
            }

            return tiles;
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
