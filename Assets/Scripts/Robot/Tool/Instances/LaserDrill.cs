using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using TileMaps.Layer;
using UnityEngine;


namespace Robot.Tool.Instances
{
    public class LaserDrill : RobotToolInstance<LaserDrillData, RobotDrillObject>
    {
        private LineRenderer lineRenderer;
        public LaserDrill(LaserDrillData toolData, RobotDrillObject robotObject) : base(toolData, robotObject)
        {
         
        }

        public override Sprite GetSprite()
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

        public override void BeginClickHold()
        {
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            lineRenderer = GameObject.Instantiate(robotObject.LineRendererPrefab,playerTransform);
        }

        public override void TerminateClickHold()
        {
            GameObject.Destroy(lineRenderer.gameObject);
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (mouseButtonKey != MouseButtonKey.Left) return;
            UpdateLineRenderer(mousePosition);
            MouseUtils.HitTileLayer(toolData.Layer, mousePosition);
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

        private void UpdateLineRenderer(Vector2 mousePosition)
        {
            Vector2 dif =  mousePosition - (Vector2) PlayerManager.Instance.GetPlayer().transform.position;
            lineRenderer.SetPositions(new Vector3[] { Vector3.up/2f, dif });
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
