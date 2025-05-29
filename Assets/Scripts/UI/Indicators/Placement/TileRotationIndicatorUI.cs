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
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace UI.Indicators.General
{
    public class TileRotationIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        [SerializeField] private Image tileImage;
        [SerializeField] private TextMeshProUGUI mRotationText;
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
            PlayerTileRotation rotation = playerScript.TilePlacementOptions.Rotation;
            int rotationValue = 0;
            
            if (rotation == PlayerTileRotation.Auto)
            {
                // TODO Display special icon
            }
            else
            {
                rotationValue = (int)playerScript.TilePlacementOptions.Rotation;
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
            
            tileImage.transform.rotation = !stateRotatable ? Quaternion.Euler(0, 0, 90 * rotationValue) : Quaternion.Euler(0, 0, 0);

            mRotationText.text = playerScript.TilePlacementOptions.Rotation.ToString().Replace("Degrees","");
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            string rotationText = playerScript.TilePlacementOptions.Rotation.ToString().Replace("Degrees","");
            if (playerScript.TilePlacementOptions.Rotation != PlayerTileRotation.Auto)
            {
                rotationText += "DEG";
            }
            string rotationMessage = $"Tile Rotation:{rotationText}";
            ToolTipController.Instance.ShowToolTip(transform.position, rotationMessage);
        }

        
        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
        

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayerTilePlacementOptions placementOptions = playerScript.TilePlacementOptions;
            int dir = eventData.button == PointerEventData.InputButton.Right ? -1 : 1;
            UpdateRotation();

            Display();
            OnPointerEnter(eventData);
            
            void UpdateRotation()
            {
                if (currentItem.tile is PlatformStateTile)
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
    }
}
