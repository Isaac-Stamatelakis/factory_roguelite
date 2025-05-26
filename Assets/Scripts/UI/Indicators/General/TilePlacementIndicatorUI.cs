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
using UnityEngine.Tilemaps;

namespace UI.Indicators.General
{
    public class TilePlacementIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator, IKeyCodeDescriptionIndicator
    {
        [SerializeField] private Image tileImage;
        private PlayerScript playerScript;
        private bool rotatable;
        private TileItem currentItem;
        private INamedStateTile currentStateTile;
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }

        public void Display([NotNull] TileItem tileItem)
        {
            rotatable = tileItem.tileOptions.rotatable || tileItem.tileType == TileType.Platform;
            currentItem = tileItem;
            currentStateTile = tileItem.tile as INamedStateTile;
            Display();
        }
        
        private void Display()
        {
            PlayerTileRotation rotation = playerScript.TilePlacementOptions.Rotation;
            int rotationValue = 0;
            if (rotatable)
            {
                if (rotation == PlayerTileRotation.Auto)
                {
                    // TODO Display special icon
                }
                else
                {
                    rotationValue = (int)playerScript.TilePlacementOptions.Rotation;
                }
            }

            bool stateRotatable = false;
            int state = playerScript.TilePlacementOptions.State;
            if (currentItem.tile is IStateTileSingle stateTileSingle)
            {
                TileBase tileBase = stateTileSingle.GetTileAtState(state);
                if (tileBase is IStateRotationTile stateRotationTile)
                {
                    stateRotatable = true;
                    tileBase = stateRotationTile.getTile(rotationValue, false);
                }
                Sprite sprite = TileItem.GetDefaultSprite(tileBase);
                tileImage.sprite = sprite;
            } else if (currentItem.tile is PlatformStateTile platformStateTile)
            {
                Sprite sprite = TileItem.GetDefaultSprite(state == 0 ? platformStateTile.GetDefaultTile() : platformStateTile.Slope);
                if (state == 0) rotationValue = 0;
                tileImage.sprite = sprite;
            }

            tileImage.color = currentItem.tileOptions.GetTileColor();
            
            if (!stateRotatable)
            {
                tileImage.transform.rotation = Quaternion.Euler(0, 0, 90 * rotationValue);
            }


        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            string rotationText = playerScript.TilePlacementOptions.Rotation.ToString().Replace("Degrees","");
            if (playerScript.TilePlacementOptions.Rotation != PlayerTileRotation.Auto)
            {
                rotationText += "DEG";
            }
            string rotationMessage = rotatable ? $"Tile Rotation:{rotationText}" : string.Empty;
            string stateMessage = currentStateTile != null ? $"Tile State:{currentStateTile.GetStateName(playerScript.TilePlacementOptions.State)}" : string.Empty;
            string message = rotationMessage;
            if (rotationMessage != string.Empty && stateMessage != string.Empty)
            {
                message += $"\n";
            }
            message += stateMessage;
            ToolTipController.Instance.ShowToolTip(transform.position, message);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
        

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayerTilePlacementOptions placementOptions = playerScript.TilePlacementOptions;
            int dir = Input.GetKey(KeyCode.LeftControl) ? -1 : 1;
            
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    UpdateRotation();
                    break;
                case PointerEventData.InputButton.Right:
                    UpdateState();
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Display();
            OnPointerEnter(eventData);

            void UpdateState()
            {
                if (currentStateTile is IRestrictedIndicatorStateTile restrictedIndicatorStateTile)
                {
                    placementOptions.State = restrictedIndicatorStateTile.ShiftState(placementOptions.State, dir);
                    return;
                }
                int maxStates = currentStateTile.GetStateAmount();
                placementOptions.State += dir;
                if (placementOptions.State > maxStates) placementOptions.State = 0;
                if (placementOptions.State < 0) placementOptions.State = maxStates;
            }

            void UpdateRotation()
            {
                if (currentStateTile is PlatformStateTile)
                {
                    placementOptions.Rotation = GlobalHelper.ShiftEnum(dir, placementOptions.Rotation);
                    bool invalid = placementOptions.Rotation is PlayerTileRotation.Degrees180 or PlayerTileRotation.Degrees270;
                    if (!invalid) return;
                    if (dir > 0)
                    {
                        placementOptions.Rotation = PlayerTileRotation.Auto;
                    }
                    else
                    {
                        placementOptions.Rotation = PlayerTileRotation.Degrees90;
                    }
                    return;
                }
                placementOptions.Rotation = GlobalHelper.ShiftEnum(dir, placementOptions.Rotation);
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
                string controlMessage = ControlUtils.FormatKeyText(GetPlayerControl());
                return
                    $"Press {controlMessage} to Switch Rotation\nPress LCtrl+{controlMessage} to Switch State\nLeft Click to Switch Rotation\nRight Click to Switch State\nHold LCtrl to Reverse";
            });
        }
    }
}
