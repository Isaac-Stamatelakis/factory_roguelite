using System;
using System.Collections.Generic;
using Chunks.Systems;
using Conduits.PortViewer;
using Conduits.Systems;
using Items;
using Player;
using Player.Controls;
using UI;
using UI.RingSelector;
using UnityEngine;

namespace Conduit.View
{
    public enum ConduitViewMode
    {
        None,
        Auto,
        All,
        WhiteList
    }
    [System.Serializable]
    public class ConduitViewOptions
    {
        public PortViewMode PortViewMode;
        public ConduitViewMode ConduitViewMode;
        public ConduitType? WhiteListType;

        public ConduitViewOptions()
        {
            PortViewMode = PortViewMode.Auto;
            ConduitViewMode = ConduitViewMode.All;
            WhiteListType = null;
        }
    }
    public class ConduitViewController : MonoBehaviour
    {
        private ConduitType? currentViewType;
        private PlayerScript playerScript;
        private ConduitTileClosedChunkSystem chunkSystem;
        public void Update()
        {
            KeyPressListen();
        }

        public void FixedUpdate()
        {
            UpdateAuto();
        }

        private void UpdateAuto()
        {
            if (playerScript.ConduitViewOptions.ConduitViewMode != ConduitViewMode.Auto) return;
            
            ConduitType? conduitType = GetSelectedConduitType();
            if (conduitType == currentViewType) return;
            currentViewType = conduitType;
            var managerDict = chunkSystem.ConduitSystemManagersDict;
            foreach (var (tileMapType, manager) in managerDict)
            {
                bool visible = manager.GetConduitType() == conduitType;
                manager.SetTileMapVisibility(visible);
            }
        }

        private void KeyPressListen()
        {
            if (!ControlUtils.GetControlKeyDown(ControlConsts.CHANGE_CONDUIT_VIEW_MODE) || ReferenceEquals(chunkSystem, null)) return;
            UIRingSelector ringSelector = GameObject.Instantiate(playerScript.Prefabs.RingSelectorPrefab);
            List<RingSelectorComponent> ringSelectorComponents = new List<RingSelectorComponent>();
                
            var modeList = new List<(Color,string,ConduitViewMode,ConduitType?)>
            {
                (Color.white,"Auto",ConduitViewMode.Auto,null),
                (Color.grey,"None",ConduitViewMode.None,null),
                (Color.green,"Item",ConduitViewMode.WhiteList,ConduitType.Item),
                (Color.blue,"Fluid",ConduitViewMode.WhiteList,ConduitType.Fluid),
                (Color.red,"Signal",ConduitViewMode.WhiteList,ConduitType.Signal),
                (Color.yellow,"Energy",ConduitViewMode.WhiteList,ConduitType.Energy),
                (Color.magenta,"Matrix",ConduitViewMode.WhiteList,ConduitType.Matrix),
            };
            foreach (var (color,elementName,active,whitelist) in modeList)
            {
                ringSelectorComponents.Add(new RingSelectorComponent(color,null,elementName,() => UpdateView(active,whitelist)));
            }
            CanvasController.Instance.PopStack();
            RingSelectorComponent defaultComponent = new RingSelectorComponent(Color.cyan,null,"All",() => UpdateView(ConduitViewMode.All,null));
            ringSelector.Display(ringSelectorComponents, defaultComponent);
            CanvasController.Instance.DisplayObject(ringSelector.gameObject);
        }

        public void UpdateView(ConduitViewMode conduitViewMode, ConduitType? whiteListType)
        {
            ConduitViewOptions viewOptions = playerScript.ConduitViewOptions;
            viewOptions.ConduitViewMode = conduitViewMode;
            viewOptions.WhiteListType = whiteListType;
            DisplayView();
        }

        public void DisplayView()
        {
            ConduitViewOptions viewOptions = playerScript.ConduitViewOptions;
            var managerDict = chunkSystem.ConduitSystemManagersDict;
            foreach (var (tileMapType, manager) in managerDict)
            {
                bool visible = IsVisibile(viewOptions, manager.GetConduitType());
                
                manager.SetTileMapVisibility(visible);
            }
        }

        private bool IsVisibile(ConduitViewOptions conduitViewOptions, ConduitType systemType)
        {
            switch (conduitViewOptions.ConduitViewMode)
            {
                case ConduitViewMode.None:
                    return false;
                case ConduitViewMode.Auto:
                    ConduitType? type = GetSelectedConduitType();
                    bool match = type == systemType;
                    if (match)
                    {
                        currentViewType = type;
                    }

                    return match;
                case ConduitViewMode.All:
                    return true;
                case ConduitViewMode.WhiteList:
                    return conduitViewOptions.WhiteListType == systemType;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ConduitType? GetSelectedConduitType()
        {
            string id = playerScript.PlayerInventory.getSelectedId();
            if (id == null) return null;
            ConduitItem conduitItem = ItemRegistry.GetInstance().GetConduitItem(id);
            return  conduitItem?.GetConduitType();
        }
        

        public void Initialize(ConduitTileClosedChunkSystem chunkSystem, PlayerScript playerScript)
        {
            this.chunkSystem = chunkSystem;
            this.playerScript = playerScript;
            DisplayView();
        }
    }
}
