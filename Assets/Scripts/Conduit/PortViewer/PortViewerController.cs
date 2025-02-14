using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule;
using Chunks.Systems;
using Conduit.View;
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
        [SerializeField] private UIRingSelector ringSelectorPrefab;
        [SerializeField] private ConduitPortTiles portConduitTiles;
        private ConduitTileClosedChunkSystem closedChunkSystem;
        private ConduitPortViewer portViewer;
        private PlayerInventory playerInventory;
        private ItemRegistry itemRegistry;
        public UIRingSelector RingSelectorPrefab => ringSelectorPrefab;
        private PlayerScript playerScript;
        
        public void Initialize(ConduitTileClosedChunkSystem closedChunkSystem, PlayerScript playerScript) {
            this.closedChunkSystem = closedChunkSystem;
            playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            itemRegistry = ItemRegistry.GetInstance();
            portViewer = GetComponentInChildren<ConduitPortViewer>();
            this.playerScript = playerScript;
            if (portViewer == null) {
                Debug.LogWarning("Conduit Port Controller has no viewer child");
            }

            ConduitViewOptions conduitViewOptions = playerScript.ConduitViewOptions;
            switch (conduitViewOptions.PortViewMode)
            {
                case PortViewMode.Item:
                    ActivateViewer(ConduitType.Item);
                    break;
                case PortViewMode.Fluid:
                    ActivateViewer(ConduitType.Fluid);
                    break;
                case PortViewMode.Energy:
                    ActivateViewer(ConduitType.Energy);
                    break;
                case PortViewMode.Signal:
                    ActivateViewer(ConduitType.Signal);
                    break;
            }
        }

        public void Update()
        {
            KeyPressListen();
            if (playerScript.ConduitViewOptions.PortViewMode == PortViewMode.Auto)
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
                (Color.grey,"Off",PortViewMode.None),
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

            CanvasController.Instance.PopStack();
            RingSelectorComponent defaultComponent = new RingSelectorComponent(Color.cyan, null, "Auto", () => ChangePortViewMode(PortViewMode.Auto));
            ringSelector.Display(ringSelectorComponents, defaultComponent);
            CanvasController.Instance.DisplayObject(ringSelector.gameObject);
        }
        
        private void ChangePortViewMode(PortViewMode portViewMode)
        {
            playerScript.ConduitViewOptions.PortViewMode = portViewMode;
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
            
            Color color = ConduitPortFactory.GetConduitPortColor(conduitType);

            Dictionary<EntityPortType,TileBase> portTypeToTile = new Dictionary<EntityPortType, TileBase>();
            portTypeToTile[EntityPortType.All] = portConduitTiles.AnyTile;
            portTypeToTile[EntityPortType.Input] = portConduitTiles.InputTile;
            portTypeToTile[EntityPortType.Output] = portConduitTiles.OutputTile;

            IConduitSystemManager conduitSystemManager = closedChunkSystem.GetManager(conduitType);
            TileMapType tileMapType = conduitType.ToTileMapType();
            TileMaps.IWorldTileMap tilemap = closedChunkSystem.GetTileMap(tileMapType);
            
            portViewer.Display(conduitSystemManager,portTypeToTile,color,tilemap,closedChunkSystem.GetPlayerChunk());
        }
        
        public void Refresh()
        {
            if (ReferenceEquals(portViewer, null)) return;
            ConduitType? currentType = portViewer.Type;
  
            if (currentType == null) return;
            portViewer.Deactive();
            ActivateViewer(currentType.Value);
        }

        
    }

    [System.Serializable]
    public struct ConduitPortTiles {
        public TileBase InputTile;
        public TileBase OutputTile;
        public TileBase AnyTile;
    }
}

