using System;
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
    public class ConduitPlacementModeIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        [SerializeField] private Image conduitImage;
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
            conduitImage.sprite = TileItem.GetDefaultSprite(current.Tile.getTileAtState(state));
            placementCounter.text = options.PlacementMode == ConduitPlacementMode.New ? placementCount.ToString() : string.Empty;
        }

        public void IterateCounter()
        {
            placementCount++;
            placementCounter.text = placementCount.ToString();
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
            ConduitPlacementOptions options = playerScript.ConduitPlacementOptions;
            int dir = eventData.button == PointerEventData.InputButton.Left ? 1 : -1;
            options.PlacementMode = GlobalHelper.ShiftEnum(dir, options.PlacementMode);
            placementCount = 0;
            Display();
            OnPointerEnter(eventData);
           
        }

        public PlayerControl? GetPlayerControl()
        {
            return PlayerControl.SwitchPlacementMode;
        }
    }
}
