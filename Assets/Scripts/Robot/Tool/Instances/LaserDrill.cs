using System;
using System.Collections.Generic;
using Chunks.Systems;
using Conduit.Conduit;
using Conduits;
using Dimensions;
using Newtonsoft.Json;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using TileMaps;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles;
using UnityEngine;


namespace Robot.Tool.Instances
{
    public interface IVeinMineEvent
    {
        public int Execute(Vector2Int initialPosition, int veinMinePower);
    }
    public abstract class VeinMineEvent<T> : IVeinMineEvent where T : IHitableTileMap
    {
        protected T hitableTileMap;
        protected VeinMineEvent(T hitableTileMap)
        {
            this.hitableTileMap = hitableTileMap;
        }

        public int Execute(Vector2Int initialPosition, int veinMinePower)
        {
            int breaks = 0;
            List<Vector2Int> directions = new List<Vector2Int>
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(initialPosition);
            while (breaks < veinMinePower && queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (!hitableTileMap.hasTile(current)) continue;
                breaks++;
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int newPosition = current + direction;
                    if (CanExpandTo(newPosition, current)) continue;
                    queue.Enqueue(newPosition);
                }

                if (DevMode.Instance.instantBreak)
                {
                    hitableTileMap.DeleteTile(new Vector2(current.x,current.y) * Global.TILE_SIZE);
                }
                else
                {
                    hitableTileMap.BreakAndDropTile(current);
                }
                
            }

            return breaks;
        }
        
        protected abstract bool CanExpandTo(Vector2Int position, Vector2Int origin);
    }

    public class BlockVeinMineEvent : VeinMineEvent<WorldTileGridMap>
    {
        private int drillPower;
        private int initialHardness;

        public BlockVeinMineEvent(WorldTileGridMap hitableTileMap, int drillPower, int initialHardness) : base(hitableTileMap)
        {
            this.drillPower = drillPower;
            this.initialHardness = initialHardness;
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            if (!hitableTileMap.hasTile(position)) return false;
            TileOptions tileOptions = hitableTileMap.getTileItem(position).tileOptions;
            return tileOptions.hardness <= initialHardness && (int) tileOptions.requiredToolTier <= drillPower;
        }
    }

    public class BackGroundVeinMineEvent : VeinMineEvent<WorldTileGridMap>
    {
        public BackGroundVeinMineEvent(WorldTileGridMap hitableTileMap) : base(hitableTileMap)
        {
           
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            return hitableTileMap.hasTile(position);
        }
    }
    public class ConduitVeinMineEvent : VeinMineEvent<ConduitTileMap>
    {
        private IConduit initialConduit;
        public ConduitVeinMineEvent(ConduitTileMap hitableTileMap, IConduit initialConduit) : base(hitableTileMap)
        {
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            IConduit originConduit = hitableTileMap.ConduitSystemManager.GetConduitAtCellPosition(origin);
            if (originConduit == null) return false;
            IConduit conduit = hitableTileMap.ConduitSystemManager.GetConduitAtCellPosition(position);
            if (conduit == null) return false;
            Vector2Int dif = origin - position;
            ConduitDirectionState? conduitDirectionState = ConduitUtils.StateFromVector2Int(dif);
            if (conduitDirectionState == null) return false;
            return originConduit.ConnectsDirection(conduitDirectionState.Value);
        }
    }
    public class LaserDrill : RobotToolInstance<LaserDrillData, RobotDrillObject>, IAcceleratedClickHandler
    {
        private LineRenderer lineRenderer;
        public LaserDrill(LaserDrillData toolData, RobotDrillObject robotObject, RobotStatLoadOutCollection loadOut) : base(toolData, robotObject, loadOut)
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
            bool hit = MouseUtils.RaycastObject(mousePosition, toolData.Layer.toRaycastLayers());
            if (!hit) return;
            
            int drillPower = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Tier);
            float veinMineUpgrades = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)RobotDrillUpgrade.VeinMine);
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMineUpgrades);
            if (TryVeinMine(mousePosition, veinMinePower, drillPower)) return;
            int multiBreak = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.MultiBreak);
            if (multiBreak == 0)
            {
                MouseUtils.HitTileLayer(toolData.Layer, mousePosition, RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier));
                return;
            }
            
            for (int x = -multiBreak; x <= multiBreak; x++)
            {
                for (int y = -multiBreak; y <= multiBreak; y++)
                {
                    Vector2 position = mousePosition + Global.TILE_SIZE * new Vector2(x, y);
                    MouseUtils.HitTileLayer(toolData.Layer, position, RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier));
                }
            }
            
            
        }

        private bool TryVeinMine(Vector2 mousePosition, int veinMinePower, int drillPower)
        {
            if (veinMinePower < 0) return false;
            
            IVeinMineEvent veinMineEvent = GetVeinMineEvent(veinMinePower,drillPower);
            if (veinMineEvent == null) return false;
            
            Vector2Int initialPosition = Global.getCellPositionFromWorld(mousePosition);
            int broken = veinMineEvent.Execute(initialPosition, veinMinePower);
            return broken > 0;
        }

        private IVeinMineEvent GetVeinMineEvent(int veinMinePower, int drillPower)
        {
            TileMapLayer tileMapLayer = toolData.Layer;
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            switch (tileMapLayer)
            {
                case TileMapLayer.Base:
                    return new BlockVeinMineEvent(closedChunkSystem.GetTileMap(TileMapType.Block) as WorldTileGridMap, drillPower, veinMinePower);
                case TileMapLayer.Background:
                    return new BackGroundVeinMineEvent(closedChunkSystem.GetTileMap(TileMapType.Background) as WorldTileGridMap);
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
