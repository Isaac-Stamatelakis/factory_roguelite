using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule;
using Chunks.Systems;
using Items;
using UnityEngine.Tilemaps;
using TileEntity;
using Conduits.Ports;
using UI;
using Conduits.Systems;
using Player;
using Player.Controls;
using PlayerModule.Mouse;
using TileMaps.Type;
using TileMaps;
using UI.RingSelector;

namespace Conduits.PortViewer {
    public enum PortViewMode
    {
        Auto,
        None,
        Item,
        Fluid,
        Energy,
        Signal,
        Matrix
    }
    public class PortViewerController : MonoBehaviour
    {
        private PortViewMode portViewMode = PortViewMode.Auto;
        [SerializeField] private UIRingSelector ringSelectorPrefab;
        [SerializeField] private ConduitPortTiles portConduitTiles;
        private ConduitTileClosedChunkSystem closedChunkSystem;
        private ConduitPortViewer portViewer;
        private PlayerInventory playerInventory;
        private ItemRegistry itemRegistry;
        
        public void Initalize(ConduitTileClosedChunkSystem closedChunkSystem) {
            this.closedChunkSystem = closedChunkSystem;
            playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            itemRegistry = ItemRegistry.GetInstance();
            portViewer = GetComponentInChildren<ConduitPortViewer>();
            if (portViewer == null) {
                Debug.LogWarning("Conduit Port Controller has no viewer child");
            }
        }

        public void Update()
        {
            KeyPressListen();
            if (portViewMode == PortViewMode.Auto)
            {
                AutoSelect();
            }
            
        }

        private void AutoSelect()
        {
            string id = playerInventory.getSelectedId();
            if (id == null) {
                portViewer.Deactive();
                return;
            }
            ConduitItem conduitItem = itemRegistry.GetConduitItem(id);
            if (ReferenceEquals(conduitItem,null)) {
                portViewer.Deactive();
                return;
            }
            if (!portViewer.Active) {
                ActivateViewer(conduitItem.GetConduitType());
                return;
            }
            if (conduitItem.GetConduitType() != portViewer.Type) {
                portViewer.Deactive();
                ActivateViewer(conduitItem.GetConduitType());
                return;
            }
        }

        private void KeyPressListen()
        {
            if (!ControlUtils.GetControlKeyDown(ControlConsts.SWITCH_CONDUIT_PORT_VIEW)) return;
            
            UIRingSelector ringSelector = GameObject.Instantiate(ringSelectorPrefab);
            List<RingSelectorComponent> ringSelectorComponents = new List<RingSelectorComponent>();
                
            List<(Color,string,PortViewMode)> modeList = new List<(Color,string,PortViewMode)>
            {
                (Color.cyan,"Auto",PortViewMode.Auto),
                (Color.green,"Item",PortViewMode.Item),
                (Color.blue,"Fluid",PortViewMode.Fluid),
                (Color.red,"Signal",PortViewMode.Signal),
                (Color.yellow,"Energy",PortViewMode.Energy),
                (Color.magenta,"Matrix",PortViewMode.Matrix),
            };
            foreach (var (color, elementName, portViewMode) in modeList)
            {
                ringSelectorComponents.Add(new RingSelectorComponent(color,null,elementName,() => ChangePortViewMode(portViewMode)));
            }
            ringSelector.Display(ringSelectorComponents, () => ChangePortViewMode(PortViewMode.None));
            CanvasController.Instance.DisplayObject(ringSelector.gameObject);
        }
        
        private void ChangePortViewMode(PortViewMode portViewMode)
        {
            this.portViewMode = portViewMode;
            PlayerManager.Instance.GetPlayer().GetComponent<PlayerMouse>().ConduitPortViewMode = portViewMode;
            switch (portViewMode)
            {
                case PortViewMode.Auto:
                    break;
                case PortViewMode.None:
                    if (portViewer.Active)
                    {
                        portViewer.Deactive();
                    }
                    break;
                case PortViewMode.Item:
                    ChangeSpecificConduitMode(ConduitType.Item);
                    break;
                case PortViewMode.Fluid:
                    ChangeSpecificConduitMode(ConduitType.Fluid);
                    break;
                case PortViewMode.Energy:
                    ChangeSpecificConduitMode(ConduitType.Energy);
                    break;
                case PortViewMode.Signal:
                    ChangeSpecificConduitMode(ConduitType.Signal);
                    break;
                case PortViewMode.Matrix:
                    ChangeSpecificConduitMode(ConduitType.Matrix);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portViewMode), portViewMode, null);
            }
        }

        private void ChangeSpecificConduitMode(ConduitType conduitType)
        {
            if (portViewer.Type != conduitType)
            {
                portViewer.Deactive();
            }
            ActivateViewer(conduitType);
        }


        private void ActivateViewer(ConduitType conduitType) {
            portViewer.name = $"{conduitType} Port Viewer";
            portViewer.gameObject.SetActive(true);
            
            Color color = getConduitPortColor(conduitType);

            Dictionary<EntityPortType,TileBase> portTypeToTile = new Dictionary<EntityPortType, TileBase>();
            portTypeToTile[EntityPortType.All] = portConduitTiles.AnyTile;
            portTypeToTile[EntityPortType.Input] = portConduitTiles.InputTile;
            portTypeToTile[EntityPortType.Output] = portConduitTiles.OutputTile;

            IConduitSystemManager conduitSystemManager = closedChunkSystem.GetManager(conduitType);
            TileMapType tileMapType = conduitType.ToTileMapType();
            TileMaps.IWorldTileMap tilemap = closedChunkSystem.GetTileMap(tileMapType);
            
            portViewer.Display(conduitSystemManager,portTypeToTile,color,tilemap);
        }

        public void Refresh()
        {
            if (ReferenceEquals(portViewer, null)) return;
            ConduitType? currentType = portViewer.Type;
  
            if (currentType == null) return;
            portViewer.Deactive();
            ActivateViewer(currentType.Value);
        }

        private Color getConduitPortColor(ConduitType conduitType) {
            switch (conduitType) {
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
            }
            throw new System.Exception($"Did not cover case for ConduitType {conduitType}");
        }
    }

    [System.Serializable]
    public struct ConduitPortTiles {
        public TileBase InputTile;
        public TileBase OutputTile;
        public TileBase AnyTile;
    }
}

