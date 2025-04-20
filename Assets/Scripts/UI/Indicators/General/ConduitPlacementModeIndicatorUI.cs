using System;
using System.Collections.Generic;
using Conduit.Placement.LoadOut;
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
    public class ConduitPlacementModeIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator,IKeyCodeDescriptionIndicator
    {
        [SerializeField] private Image conduitImage;
        [SerializeField] private ConduitLoadOutEditorUI conduitLoadOutEditorUIPrefab;
        [SerializeField] private TextMeshProUGUI placementCounter;
        private PlayerScript playerScript;
        private ConduitItem current;
        private int placementCount;
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }


        public void Display(ConduitItem conduitItem)
        {
            current = conduitItem;
            Display();
        }

        public void Display()
        {
            ConduitPlacementOptions options = playerScript.ConduitPlacementOptions;
            int state = options.PlacementMode == ConduitPlacementMode.Any 
                ? (int)ConduitDirectionState.Up + (int)ConduitDirectionState.Down + (int)ConduitDirectionState.Left + (int)ConduitDirectionState.Right 
                : 0;
            state += (int)ConduitDirectionState.Active; 
            conduitImage.sprite = TileItem.GetDefaultSprite(current?.Tile.getTileAtState(state));
            placementCounter.text = options.PlacementMode == ConduitPlacementMode.New ? placementCount.ToString() : string.Empty;
        }

        public void IterateCounter()
        {
            placementCount++;
            placementCounter.text = playerScript.ConduitPlacementOptions.PlacementMode == ConduitPlacementMode.New ? placementCount.ToString() : string.Empty;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Conduit Placement Mode:Connect {playerScript.ConduitPlacementOptions.PlacementMode}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            if (!current) return;
            ConduitPlacementOptions options = playerScript.ConduitPlacementOptions;
            
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    options.PlacementMode = GlobalHelper.ShiftEnum(1, options.PlacementMode);
                    break;
                case PointerEventData.InputButton.Right:
                    LoadOutConduitType? loadOutConduitType = GetLoadOutType(current.GetConduitType());
                    if (!loadOutConduitType.HasValue) return;
                    
                    ConduitLoadOutEditorUI loadOutEditorUI = Instantiate(conduitLoadOutEditorUIPrefab);
                    loadOutEditorUI.Display(playerScript.ConduitPlacementOptions.ConduitPlacementLoadOuts,loadOutConduitType.Value);
                    CanvasController.Instance.DisplayObject(loadOutEditorUI.gameObject);
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            placementCount = 0;
            Display();
            OnPointerEnter(eventData);
           
        }

        private LoadOutConduitType? GetLoadOutType(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Item:
                case ConduitType.Fluid:
                    return LoadOutConduitType.ItemFluid;
                case ConduitType.Energy:
                    return LoadOutConduitType.Energy;
                case ConduitType.Signal:
                    return LoadOutConduitType.Signal;
                case ConduitType.Matrix:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
            }
        }


        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.SwitchPlacementMode;
        }

        public void SyncToolTipDisplayer(ToolTipUIDisplayer toolTipUIDisplayer)
        {
            toolTipUIDisplayer.SetAction(() =>
            {
                List<KeyCode> keyCodes = ControlUtils.GetKeyCodes(GetPlayerControl());
                string controlMessage = ControlUtils.KeyCodeListAsString(keyCodes, "+");
                return $"Press {controlMessage} to Toggle Conduit Placement Mode\nPress LCtrl+{controlMessage} to Terminate Placement Group\nLeft Click to Switch Conduit Mode\nRight Click to Modify Default Conduit Ports";
            });
        }
    }
}
