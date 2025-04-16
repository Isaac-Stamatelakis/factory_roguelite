using System;
using System.Collections.Generic;
using System.Linq;
using Conduits;
using Dimensions;
using Item.Slot;
using Items;
using Player;
using Player.Mouse;
using Player.Tool.Object;
using PlayerModule;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.Instances.VeinMine;
using Robot.Upgrades.LoadOut;
using TileMaps;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Robot.Tool.Instances
{
    public class ConduitCutters : RobotToolInstance<ConduitCuttersData, RobotConduitCutterObject>, IDestructiveTool
    {
        private MultiButtonRobotToolLaserManager laserManager;
        private List<ConduitType> targets = new List<ConduitType>(5);
        public ConduitCutters(ConduitCuttersData toolData, RobotConduitCutterObject robotObject, RobotStatLoadOutCollection loadOut, PlayerScript playerScript) : base(toolData, robotObject, loadOut, playerScript)
        {
            laserManager = new MultiButtonRobotToolLaserManager(base.playerScript, robotObject.LineRendererPrefab);
            SetTargets();
        }
        
        public override Sprite GetPrimaryModeSprite()
        {
            return toolData.Type switch
            {
                ConduitCutterMode.Item => robotObject.ItemLayerSprite,
                ConduitCutterMode.Fluid => robotObject.FluidLayerSprite,
                ConduitCutterMode.Energy => robotObject.EnergyLayerSprite,
                ConduitCutterMode.Signal => robotObject.SignalLayerSprite,
                ConduitCutterMode.Matrix => robotObject.MatrixLayerSprite,
                ConduitCutterMode.All => robotObject.SignalLayerSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            laserManager.Update(ref mousePosition,GetConduitColor(toolData.Type), mouseButtonKey);
            laserManager.SetMaterial(toolData.Type == ConduitCutterMode.All ? robotObject.RainbowShader : null);
        }

        
        public override void TerminateClickHold(MouseButtonKey mouseButtonKey)
        {
            laserManager.DeActivate(mouseButtonKey);
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (!playerRobot.TryConsumeEnergy(RobotConduitUpgradeInfo.COST_PER_HIT,0.1f)) return;
            laserManager.Update(ref mousePosition,GetConduitColor(toolData.Type), mouseButtonKey);
            switch (mouseButtonKey)
            {
                case MouseButtonKey.Left:
                    BreakConduit(mousePosition);
                    break;
                case MouseButtonKey.Right:
                    if (!Input.GetMouseButtonDown(mouseButtonKey.ToMouseButton())) return;
                    DisconnectConduits(mousePosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private void DisconnectConduits(Vector2 mousePosition)
        {
            foreach (ConduitType target in targets)
            {
                IWorldTileMap iWorldTileMap = playerScript.CurrentSystem.GetTileMap(target.ToTileMapType());
                if (iWorldTileMap is not ConduitTileMap conduitTileMap) continue;
                conduitTileMap.DisconnectConduits(mousePosition);
            }
        }

        private void BreakConduit(Vector2 mousePosition)
        {
            float veinMine = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)ConduitSlicerUpgrade.VeinMine);
            
            foreach (ConduitType target in targets)
            {
                int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMine);
                ConduitTileMap conduitTileMap = DimensionManager.Instance.GetPlayerSystem().GetTileMap(target.ToTileMapType()) as ConduitTileMap;
                if (!conduitTileMap) continue;
                Vector2Int cellPosition = Global.GetCellPositionFromWorld(mousePosition);
                IConduit conduit = conduitTileMap.ConduitSystemManager.GetConduitAtCellPosition(cellPosition);
                if (conduit == null) continue;
                bool drop = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)ConduitSlicerUpgrade.Item_Magnet) == 0;
            
                MouseUtils.HitTileLayer(target.ToTileMapType().ToLayer(), mousePosition,drop,0,true);
                if (!drop)
                {
                    PlayerInventory playerInventory = playerScript.PlayerInventory;
                    playerInventory.Give(new ItemSlot(conduit?.GetConduitItem(),1,null));
                }
                if (veinMinePower <= 1) continue;

                bool EnergyCostFunction()
                {
                    return playerRobot.TryConsumeEnergy(32, 0.1f);
                }
                ConduitVeinMineEvent veinMineEvent = new ConduitVeinMineEvent(conduitTileMap, false, EnergyCostFunction,conduit);
                veinMineEvent.Execute(cellPosition,veinMinePower);
                if (!drop)
                {
                    PlayerInventory playerInventory = playerScript.PlayerInventory;
                    playerInventory.GiveItems(veinMineEvent.GetCollectedItems());
                }
            }
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            List<ConduitCutterMode> orderedModes = new List<ConduitCutterMode>
            {
                ConduitCutterMode.Matrix,
                ConduitCutterMode.Energy,
                ConduitCutterMode.Signal,
                ConduitCutterMode.Item,
                ConduitCutterMode.Fluid,
                ConduitCutterMode.All
            };
            int index = orderedModes.IndexOf(toolData.Type);
            int dir = moveDirection == MoveDirection.Left ? 1 : -1;
            index += dir;
            if (index < 0) index = orderedModes.Count - 1;
            if (index >= orderedModes.Count) index = 0;
            toolData.Type = orderedModes[index];
            SetTargets();
            laserManager.SetMaterial(toolData.Type == ConduitCutterMode.All ? robotObject.RainbowShader : null);
        }

        private void SetTargets()
        {
            targets = toolData.Type == ConduitCutterMode.All 
                ? Enum.GetValues(typeof(ConduitType)).Cast<ConduitType>().ToList() 
                : new List<ConduitType> { (ConduitType)toolData.Type };
        }

        public override string GetModeName()
        {
            return toolData?.Type.ToString();
        }

        public override void Preview(Vector2Int cellPosition)
        {
            
        }
        

        private Color GetConduitColor(ConduitCutterMode conduitCutterMode)
        {
            switch (conduitCutterMode)
            {
                case ConduitCutterMode.Item:
                    return Color.green;
                case ConduitCutterMode.Fluid:
                    return Color.blue;
                case ConduitCutterMode.Energy:
                    return Color.yellow;
                case ConduitCutterMode.Signal:
                    return Color.red;
                case ConduitCutterMode.Matrix:
                    return Color.magenta;
                case ConduitCutterMode.All:
                    return Color.white;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitCutterMode), conduitCutterMode, null);
            }
        }
    }

    public enum ConduitCutterMode
    {
        Item = ConduitType.Item,
        Fluid = ConduitType.Fluid,
        Energy = ConduitType.Energy,
        Signal = ConduitType.Signal,
        Matrix = ConduitType.Matrix,
        All = 5
    }

    public class ConduitCuttersData : RobotToolData
    {
        public ConduitCutterMode Type;
    }
}
