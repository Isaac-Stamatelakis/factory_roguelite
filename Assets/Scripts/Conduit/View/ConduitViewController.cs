using System.Collections.Generic;
using Chunks.Systems;
using Conduits.PortViewer;
using Conduits.Systems;
using Player.Controls;
using UI;
using UI.RingSelector;
using UnityEngine;

namespace Conduit.View
{
    public class ConduitViewMode
    {
        public bool Active;
        public ConduitType? WhiteListType;
    }
    public class ConduitViewController : MonoBehaviour
    {
        private ConduitTileClosedChunkSystem chunkSystem;
        public void Update()
        {
            if (!ControlUtils.GetControlKeyDown(ControlConsts.CHANGE_CONDUIT_VIEW_MODE) || ReferenceEquals(chunkSystem, null)) return;
            UIRingSelector ringSelector = GameObject.Instantiate(chunkSystem.PortViewerController.RingSelectorPrefab); // TODO Change this its scuffed as fuck
            List<RingSelectorComponent> ringSelectorComponents = new List<RingSelectorComponent>();
                
            var modeList = new List<(Color,string,bool,ConduitType?)>
            {
                (Color.grey,"None",false,null),
                (Color.green,"Item",true,ConduitType.Item),
                (Color.blue,"Fluid",true,ConduitType.Fluid),
                (Color.red,"Signal",true,ConduitType.Signal),
                (Color.yellow,"Energy",true,ConduitType.Energy),
                (Color.magenta,"Matrix",true,ConduitType.Matrix),
            };
            foreach (var (color,elementName,active,whitelist) in modeList)
            {
                ringSelectorComponents.Add(new RingSelectorComponent(color,null,elementName,() => UpdateView(active,whitelist)));
            }
            RingSelectorComponent defaultComponent = new RingSelectorComponent(Color.cyan,null,"All",() => UpdateView(true,null));
            ringSelector.Display(ringSelectorComponents, defaultComponent);
            CanvasController.Instance.DisplayObject(ringSelector.gameObject);
        }

        public void UpdateView(bool active, ConduitType? whiteListType)
        {
            var managerDict = chunkSystem.ConduitSystemManagersDict;
            foreach (var (tileMapType, manager) in managerDict)
            {
                bool activate = active && (whiteListType == null || whiteListType == manager.GetConduitType());
                manager.SetTileMapVisibility(activate);
            }
            
           
        }
        

        public void Initialize(ConduitTileClosedChunkSystem chunkSystem)
        {
            this.chunkSystem = chunkSystem;
        }
    }
}
