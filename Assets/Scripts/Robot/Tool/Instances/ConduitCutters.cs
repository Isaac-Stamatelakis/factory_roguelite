using System;
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
        public ConduitCutters(ConduitCuttersData toolData, RobotConduitCutterObject robotObject, RobotStatLoadOutCollection loadOut, PlayerScript playerScript) : base(toolData, robotObject, loadOut, playerScript)
        {
            laserManager = new MultiButtonRobotToolLaserManager(base.playerScript, robotObject.LineRendererPrefab);
        }
        
        public override Sprite GetPrimaryModeSprite()
        {
            return toolData.Type switch
            {
                ConduitType.Item => robotObject.ItemLayerSprite,
                ConduitType.Fluid => robotObject.FluidLayerSprite,
                ConduitType.Energy => robotObject.EnergyLayerSprite,
                ConduitType.Signal => robotObject.SignalLayerSprite,
                ConduitType.Matrix => robotObject.MatrixLayerSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            laserManager.Update(ref mousePosition,GetConduitColor(toolData.Type), mouseButtonKey);
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
                    IWorldTileMap iWorldTileMap = playerScript.CurrentSystem.GetTileMap(toolData.Type.ToTileMapType());
                    if (iWorldTileMap is not ConduitTileMap conduitTileMap) return;
                    conduitTileMap.DisconnectConduits(mousePosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private void BreakConduit(Vector2 mousePosition)
        {
            float veinMine = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)ConduitSlicerUpgrade.VeinMine);
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(veinMine);
            ConduitTileMap conduitTileMap = DimensionManager.Instance.GetPlayerSystem().GetTileMap(toolData.Type.ToTileMapType()) as ConduitTileMap;
            if (!conduitTileMap) return;
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(mousePosition);
            IConduit conduit = conduitTileMap.ConduitSystemManager.GetConduitAtCellPosition(cellPosition);
            if (conduit == null) return;
            bool drop = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)ConduitSlicerUpgrade.Item_Magnet) == 0;
            
            MouseUtils.HitTileLayer(toolData.Type.ToTileMapType().ToLayer(), mousePosition,drop,0,true);
            if (!drop)
            {
                PlayerInventory playerInventory = playerScript.PlayerInventory;
                playerInventory.Give(new ItemSlot(conduit?.GetConduitItem(),1,null));
            }
            if (veinMinePower <= 1) return;

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

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            toolData.Type = GlobalHelper.ShiftEnum(moveDirection == MoveDirection.Left ? 1 : -1, toolData.Type);
            
        }

        public override string GetModeName()
        {
            return toolData?.Type.ToString();
        }

        public override void Preview(Vector2Int cellPosition)
        {
            
        }
        

        private Color GetConduitColor(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Item:
                    return Color.green;
                case ConduitType.Fluid:
                    return Color.blue;
                case ConduitType.Energy:
                    return Color.yellow;
                case ConduitType.Signal:
                    return Color.red;
                case ConduitType.Matrix:
                    return Color.magenta;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
            }
        }
    }

    public class ConduitCuttersData : RobotToolData
    {
        public ConduitType Type;

    }
}
