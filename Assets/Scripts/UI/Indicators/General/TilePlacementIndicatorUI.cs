using System;
<<<<<<< HEAD
using System.Collections.Generic;
=======
>>>>>>> main
using Items;
using Player;
using Player.Controls;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using JetBrains.Annotations;
using Tiles;
using UnityEngine.Tilemaps;

namespace UI.Indicators.General
{
<<<<<<< HEAD
    public class TilePlacementIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator, IKeyCodeDescriptionIndicator
=======
    public class TilePlacementIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
>>>>>>> main
    {
        [SerializeField] private Image tileImage;
        private PlayerScript playerScript;
        private bool rotatable;
        private bool stateModifiable;
        private TileItem currentItem;
        
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
        }

        public void Display([NotNull] TileItem tileItem)
        {
            rotatable = tileItem.tileOptions.rotatable;
            stateModifiable = tileItem.tile is IStateTile;
            currentItem = tileItem;
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
            if (stateModifiable)
            {
                int state = playerScript.TilePlacementOptions.State;
                IStateTile stateTile = (IStateTile) currentItem.tile;
                TileBase tileBase = stateTile.getTileAtState(state);
                if (tileBase is IStateRotationTile stateRotationTile)
                {
                    stateRotatable = true;
                    tileBase = stateRotationTile.getTile(rotationValue, false);
                }
                Sprite sprite = TileItem.GetDefaultSprite(tileBase);
                tileImage.sprite = sprite;
            }
           
            tileImage.color = currentItem.tileOptions.TileColor 
             ? currentItem.tileOptions.TileColor.GetColor()
                : Color.white;

            if (!stateRotatable)
            {
                tileImage.transform.rotation = Quaternion.Euler(0,0 ,90*rotationValue);
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
            string stateMessage = stateModifiable ? $"Tile State:{GetStateName(playerScript.TilePlacementOptions.State)}" : string.Empty;
            string message = rotationMessage;
            if (rotationMessage != String.Empty && stateMessage != string.Empty)
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
        
        private string GetStateName(int state)
        {
            switch (state)
            {
                case HammerTile.BASE_TILE_STATE:
                    return "Tile";
                case HammerTile.SLAB_TILE_STATE:
                    return "Slab";
                case HammerTile.SLANT_TILE_STATE:
                    return "Slant";
                case HammerTile.STAIR_TILE_STATE:
                    return "Stair";
                default:
                    return null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayerTilePlacementOptions placementOptions = playerScript.TilePlacementOptions;
            int dir = Input.GetKey(KeyCode.LeftControl) ? -1 : 1;
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    placementOptions.Rotation = GlobalHelper.ShiftEnum(dir, placementOptions.Rotation);
                    break;
                case PointerEventData.InputButton.Right:
                    const int MAX_STATE = 3;
                    placementOptions.State += dir; 
                    if (placementOptions.State > MAX_STATE) placementOptions.State = 0;
                    if (placementOptions.State < 0) placementOptions.State = MAX_STATE;
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Display();
            OnPointerEnter(eventData);
        }

<<<<<<< HEAD
        public PlayerControl GetPlayerControl()
=======
        public PlayerControl? GetPlayerControl()
>>>>>>> main
        {
            return PlayerControl.SwitchPlacementMode;
        }

<<<<<<< HEAD
        public void SyncToolTipDisplayer(ToolTipUIDisplayer toolTipUIDisplayer)
        {
            
            toolTipUIDisplayer.SetAction(() =>
            {
                List<KeyCode> keyCodes = ControlUtils.GetKeyCodes(GetPlayerControl());
                string controlMessage = ControlUtils.KeyCodeListAsString(keyCodes, "+");
                return $"Press {controlMessage} to Switch Rotation\nPress LCtrl+{controlMessage} to Switch State\nLeft Click to Switch Rotation\nRight Click to Switch State\nHold LCtrl to Reverse";
            });
=======
        public KeyCode GetOptionalKeyCode()
        {
            return KeyCode.LeftControl;
>>>>>>> main
        }
    }
}
