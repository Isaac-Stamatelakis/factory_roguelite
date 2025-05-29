using System;
using System.Collections.Generic;
using Conduit.Placement.LoadOut;
using Conduits.Ports;
using Conduits.Systems;
using Items;
using Player;
using Player.Controls;
using Tiles;
using TMPro;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class ConduitLoadoutIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator,IKeyCodeDescriptionIndicator
    {
        [SerializeField] private Image mInput;
        [SerializeField] private Image mOutput;
        [SerializeField] private Image mInputColor;
        [SerializeField] private Image mOutputColor;
        
        private PlayerScript playerScript;
        
        private int placementCount;
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }
        
        public void Display(LoadOutConduitType conduitType)
        {
            IOConduitPortData ioConduitPortData = playerScript.ConduitPlacementOptions.ConduitPlacementLoadOuts[conduitType];
            bool inputActive = ioConduitPortData.InputData.Enabled;
            bool outputActive=  ioConduitPortData.OutputData.Enabled;
            Color inputColor = ConduitPortFactory.GetColorFromInt(ioConduitPortData.InputData.Color);
            Color outputColor =  ConduitPortFactory.GetColorFromInt(ioConduitPortData.OutputData.Color);
            
            mInput.color = inputActive ? Color.green :  Color.red;
            mOutput.color = outputActive ? Color.green : Color.red;
            mInputColor.color = inputColor;
            mOutputColor.color = outputColor;
        }

        public void IterateCounter()
        {
            
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Conduit Placement Mode:Connect {playerScript.ConduitPlacementOptions.ConduitPlacementLoadOuts}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }

        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.SwapRobotLoadOut;
        }

        public void SyncToolTipDisplayer(ToolTipUIDisplayer toolTipUIDisplayer)
        {
            toolTipUIDisplayer.SetAction(() =>
            {
                string controlMessage = ControlUtils.FormatInputText(GetPlayerControl());
                return $"Press {controlMessage} to Toggle Conduit Placement Mode\nPress LCtrl+{controlMessage} to Terminate Placement Group\nLeft Click to Switch Conduit Mode\nRight Click to Modify Default Conduit Ports";
            });
        }
    }
}
