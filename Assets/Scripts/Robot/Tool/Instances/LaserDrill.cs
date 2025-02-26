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
        public int Execute(Vector2Int initial, int veinMinePower);
    }
    public abstract class VeinMineEvent<T> : IVeinMineEvent where T : IHitableTileMap
    {
        protected T hitableTileMap;
        protected Vector2Int initialPosition;
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down,
        };
        protected VeinMineEvent(T hitableTileMap, Vector2Int initialPosition)
        {
            this.hitableTileMap = hitableTileMap;
            this.initialPosition = initialPosition;
        }

        public int Execute(Vector2Int initial, int veinMinePower)
        {
            Expand(initialPosition);
            int breaks = 0;
            
            while (breaks < veinMinePower && queue.Count > 0)
            {
                bool broken = TryIterate();
                if (!broken) breaks++;
            }
            
            return breaks;
        }
        

        private bool TryIterate()
        {
            Vector2Int current = queue.Dequeue();
            
            if (!hitableTileMap.hasTile(current)) return false;
            if (DevMode.Instance.instantBreak)
            {
                hitableTileMap.DeleteTile(new Vector2(current.x,current.y) * Global.TILE_SIZE);
            }
            else
            {
                hitableTileMap.BreakAndDropTile(current);
            }
            Expand(current);

            return true;
        }

        private void Expand(Vector2Int current)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int newPosition = current + direction;
                if (!CanExpandTo(newPosition, current)) continue;
                queue.Enqueue(newPosition);
            }
        }
        protected abstract bool CanExpandTo(Vector2Int position, Vector2Int origin);
    }

    public class BlockVeinMineEvent : VeinMineEvent<WorldTileGridMap>
    {
        private int drillPower;
        private int initialHardness;

        public BlockVeinMineEvent(WorldTileGridMap hitableTileMap, Vector2Int initial, int drillPower, int initialHardness) : base(hitableTileMap, initial)
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
        public BackGroundVeinMineEvent(WorldTileGridMap hitableTileMap, Vector2Int initial) : base(hitableTileMap, initial)
        {
           
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            return hitableTileMap.hasTile(position);
        }
    }
    public class ConduitVeinMineEvent : VeinMineEvent<ConduitTileMap>
    {
        public ConduitVeinMineEvent(ConduitTileMap hitableTileMap, Vector2Int initial) : base(hitableTileMap, initial)
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

            if (toolData.Layer == TileMapLayer.Base)
            {
                bool hit = MouseUtils.RaycastObject(mousePosition, toolData.Layer.toRaycastLayers());
                if (!hit) return;
            }

            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            WorldTileGridMap worldTileGridMap = null;
            switch (toolData.Layer)
            {
                case TileMapLayer.Base:
                    worldTileGridMap = closedChunkSystem.GetTileMap(TileMapType.Block) as WorldTileGridMap;
                    break;
                case TileMapLayer.Background:
                    worldTileGridMap = closedChunkSystem.GetTileMap(TileMapType.Background) as WorldTileGridMap;
                    break;
                default:
                    break;
            }

            if (!worldTileGridMap) return;
            
            int drillPower = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.Tier);
            float veinMineUpgrades = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)RobotDrillUpgrade.VeinMine);
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMineUpgrades);
            
            int multiBreak = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)RobotDrillUpgrade.MultiBreak);
            if (multiBreak == 0)
            {
                TileItem tileItem = worldTileGridMap.getTileItem(mousePosition);
                bool broken = MouseUtils.HitTileLayer(toolData.Layer, mousePosition, RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection,(int)RobotDrillUpgrade.Tier));
                if (broken)
                {
                    TryVeinMine(worldTileGridMap, tileItem, mousePosition, veinMinePower, drillPower);
                }
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

        private bool TryVeinMine(WorldTileGridMap worldTileGridMap, TileItem initialItem, Vector2 mousePosition, int veinMinePower, int drillPower)
        {
            if (veinMinePower <= 1) return false;
            Vector2Int cellPosition = Global.getCellPositionFromWorld(mousePosition);
            IVeinMineEvent veinMineEvent = GetVeinMineEvent(worldTileGridMap, initialItem, drillPower, cellPosition);
            int? broken = veinMineEvent?.Execute(cellPosition, veinMinePower);
            return broken > 0;
            /*
            
            List<Vector2Int> directions = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
            };
            
            List<IVeinMineEvent> veinMineEvents = new List<IVeinMineEvent>(); // Vein mine events try to break the initial block so this is required to get around that
            Vector2Int cellPosition = Global.getCellPositionFromWorld(mousePosition);
            foreach (Vector2Int direction in directions)
            {
                
                IVeinMineEvent veinMineEvent = GetVeinMineEvent(drillPower, cellPosition + direction);
                if (veinMineEvent == null) continue;
                veinMineEvents.Add(veinMineEvent);
            }

            if (veinMineEvents.Count == 0) return false;
            int broken = 0;
            int index = 0;
            int noBreakIterations = 0;
            while (broken < veinMinePower)
            {
                IVeinMineEvent veinMineEvent = veinMineEvents[index];
                if (veinMineEvent.Iterate())
                {
                    broken++;
                }
                else
                {
                    noBreakIterations++;
                }
                if (noBreakIterations >= veinMineEvents.Count) break;
                index++;
                index %= veinMineEvents.Count;
            }
            */
            
        }

        private IVeinMineEvent GetVeinMineEvent(WorldTileGridMap worldTileGridMap, TileItem initialTile, int drillPower, Vector2Int current)
        {
            TileMapLayer tileMapLayer = toolData.Layer;
            switch (tileMapLayer)
            {
                case TileMapLayer.Base:
                    int hardness = initialTile.tileOptions.hardness;
                    return new BlockVeinMineEvent(worldTileGridMap, current, drillPower, hardness);
                case TileMapLayer.Background:
                    return new BackGroundVeinMineEvent(worldTileGridMap, current);
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
