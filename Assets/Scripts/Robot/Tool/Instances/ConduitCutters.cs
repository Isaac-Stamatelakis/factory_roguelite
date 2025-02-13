using System;
using Dimensions;
using Items;
using Player.Mouse;
using Player.Tool.Object;
using TileMaps;
using TileMaps.Conduit;
using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Robot.Tool.Instances
{
    public class ConduitCutters : RobotToolInstance<ConduitCuttersData, RobotConduitCutterObject>
    {
        
        private LineRenderer lineRenderer;
        public ConduitCutters(ConduitCuttersData toolData, RobotConduitCutterObject robotObject) : base(toolData, robotObject)
        {
        }

        public override Sprite GetSprite()
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
            switch (toolData.CutterMode)
            {
                case ConduitCutterMode.Standard:
                    MouseUtils.HitTileLayer(toolData.Type.ToTileMapType().toLayer(), mousePosition);
                    break;
                case ConduitCutterMode.Disconnect:
                    if (!Input.GetMouseButtonDown((int)mouseButtonKey)) return;
                    IWorldTileMap iWorldTileMap = DimensionManager.Instance.GetPlayerSystem().GetTileMap(toolData.Type.ToTileMapType()
                    );
                    if (iWorldTileMap is not ConduitTileMap conduitTileMap) return;
                    conduitTileMap.DisconnectConduits(mousePosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            if (!subMode)
            {
                toolData.Type = GlobalHelper.ShiftEnum(moveDirection == MoveDirection.Left ? 1 : -1, toolData.Type);
            }
            else
            {
                toolData.CutterMode = GlobalHelper.ShiftEnum(moveDirection == MoveDirection.Left ? 1 : -1, toolData.CutterMode);
            }
            
        }
        
        

        private void UpdateLineRenderer(Vector2 mousePosition)
        {
            Vector2 dif =  mousePosition - (Vector2) PlayerManager.Instance.GetPlayer().transform.position;
            lineRenderer.SetPositions(new Vector3[] { Vector3.up/2f, dif });
            
            Gradient gradient = lineRenderer.colorGradient;
            GradientColorKey[] colorKeys = gradient.colorKeys;
            colorKeys[1].color = GetConduitColor(toolData.Type);
            gradient.colorKeys = colorKeys;
            lineRenderer.colorGradient = gradient;
            
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
        public ConduitCutterMode CutterMode;

    }

    public enum ConduitCutterMode
    {
        Standard,
        Disconnect
    }
}
