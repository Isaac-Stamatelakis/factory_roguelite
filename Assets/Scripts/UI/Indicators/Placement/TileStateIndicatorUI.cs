using System;
using System.Collections.Generic;
using Items;
using Player;
using Player.Controls;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using JetBrains.Annotations;
using Tiles;
using Tiles.CustomTiles.StateTiles.Instances.Platform;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace UI.Indicators.General
{
    public class TileStateIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        [SerializeField] private Image tileImage;
        private PlayerScript playerScript;
        
        private TileItem currentItem;
        
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }

        public void Display([NotNull] TileItem tileItem)
        {
            currentItem = tileItem;
            Display();
        }
        
        private void Display()
        {
            int state = playerScript.TilePlacementOptions.State;
            if (currentItem.tile is IStateTileSingle stateTileSingle)
            {
                TileBase tileBase = stateTileSingle.GetTileAtState(state);
                if (tileBase is IStateRotationTile stateRotationTile)
                {
                    tileBase = stateRotationTile.getTile(0, false);
                }
                Sprite sprite = TileItem.GetDefaultSprite(tileBase);
                tileImage.sprite = sprite;
            } else if (currentItem.tile is PlatformStateTile platformStateTile)
            {
                Sprite sprite = TileItem.GetDefaultSprite(state == 0 ? platformStateTile.GetDefaultTile() : platformStateTile.Slope);
                tileImage.sprite = sprite;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            string stateMessage = currentItem.tile is INamedStateTile currentStateTile ? $"Tile State:{currentStateTile.GetStateName(playerScript.TilePlacementOptions.State)}" : string.Empty;
            ToolTipController.Instance.ShowToolTip(transform.position, stateMessage);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
        

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayerTilePlacementOptions placementOptions = playerScript.TilePlacementOptions;
            int dir = Keyboard.current.ctrlKey.isPressed ? -1 : 1;
            UpdateState();

            Display();
            OnPointerEnter(eventData);

            return;
            void UpdateState()
            {
                if (currentItem.tile is IRestrictedIndicatorStateTile restrictedIndicatorStateTile)
                {
                    placementOptions.State = restrictedIndicatorStateTile.ShiftState(placementOptions.State, dir);
                    return;
                }

                if (currentItem.tile is not IStateTile stateTile) return; 
                
                int maxStates = stateTile.GetStateAmount();
                placementOptions.State += dir;
                if (placementOptions.State > maxStates) placementOptions.State = 0;
                if (placementOptions.State < 0) placementOptions.State = maxStates;
            }
        }
        
        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.SwitchPlacementMode;
        }
        
    }
}
