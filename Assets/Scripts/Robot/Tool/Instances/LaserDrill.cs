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


namespace Robot.Tool.Instances
{
    public class LaserDrill : RobotToolInstance<LaserDrillData, RobotDrillObject>, IAcceleratedClickHandler, IDestructiveTool
    {
        private LineRenderer lineRenderer;
        public LaserDrill(LaserDrillData toolData, RobotDrillObject robotObject, RobotStatLoadOutCollection loadOut, PlayerScript playerScript) : base(toolData, robotObject, loadOut, playerScript)
        {
         
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

            if (toolData.Layer == TileMapLayer.Base)
            {
                bool hit = MouseUtils.RaycastObject(mousePosition, toolData.Layer.toRaycastLayers());
                if (!hit) return;
            }

            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            WorldTileGridMap worldTileGridMap = GetWorldTileGridMap(closedChunkSystem);
            
            bool drop = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Item_Magnet) == 0;
            
            if (!worldTileGridMap) return;
            
            int drillPower = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Tier);
            float veinMineUpgrades = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)RobotDrillUpgrade.VeinMine);
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMineUpgrades);
            
            int multiBreak = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.MultiBreak);
            if (multiBreak == 0)
            {
                TileItem tileItem = worldTileGridMap.getTileItem(mousePosition);
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
                    TileItem tileItem = worldTileGridMap.getTileItem(position);
                    bool broken = MouseUtils.HitTileLayer(toolData.Layer, position, drop,RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier),false);
                    if (broken && !drop)
                    {
                        anyBroken = true;
                        playerInventory.GiveItems(ItemSlotUtils.GetTileItemDrop(tileItem));
                    }
                }
            }

            if (anyBroken)
            {
                playerScript.PlayerMouse.UpdateOnToolChange();
            }
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
            if (mouseButtonKey != MouseButtonKey.Left) return false;
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
            Debug.Log(cellPosition);
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
            
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            int multiBreak = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.MultiBreak);
            WorldTileGridMap worldTileGridMap = GetWorldTileGridMap(closedChunkSystem);
            if (multiBreak == 0)
            {
                /*
                TileItem tileItem = worldTileGridMap.getTileItem(mousePosition);
                bool broken = MouseUtils.HitTileLayer(toolData.Layer, mousePosition, drop, RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier));
                if (broken)
                {
                    if (!drop && !DevMode.Instance.instantBreak)
                    {
                        List<ItemSlot> itemDrops = ItemSlotUtils.GetTileItemDrop(tileItem);
                        playerScript.PlayerInventory.GiveItems(itemDrops);
                    }
                    TryVeinMine(worldTileGridMap, tileItem, drop, mousePosition, veinMinePower, drillPower);
                    
                }
                */
                return;
            }
            
            Dictionary<Vector2Int, OutlineTileMapCellData> tiles = new Dictionary<Vector2Int, OutlineTileMapCellData>();
            IOutlineTileGridMap outlineTileGridMap = worldTileGridMap as IOutlineTileGridMap;
            if (outlineTileGridMap == null) return;
            for (int x = -multiBreak; x <= multiBreak; x++)
            {
                for (int y = -multiBreak; y <= multiBreak; y++)
                {
                    Vector2Int breakPosition = cellPosition + new Vector2Int(x, y);
                    tiles[breakPosition] = outlineTileGridMap.GetOutlineCellData(new Vector3Int(breakPosition.x, breakPosition.y, 0));
                }
            }
            TileBreakHighlighter tileBreakHighlighter = playerScript.TileViewers.TileBreakHighlighter;
            tileBreakHighlighter.Display(tiles);
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
