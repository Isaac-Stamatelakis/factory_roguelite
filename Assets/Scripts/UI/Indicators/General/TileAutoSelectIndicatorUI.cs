using System;
using Player.Controls;
using PlayerModule.Mouse;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class TileAutoSelectIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IKeyCodeIndicator
    {
        [System.Serializable]
        private class AutoSelectSprites
        {
            public Sprite NoAutoSelect;
            public Sprite ToolAutoSelect;
            public Sprite TileEntityAutoSelect;
            public Sprite GetSprite(PlayerMouse.AutoSelectMode autoSelectMode)
            {
                switch (autoSelectMode)
                {
                    case PlayerMouse.AutoSelectMode.None:
                        return NoAutoSelect;
                    case PlayerMouse.AutoSelectMode.Tool:
                        return ToolAutoSelect;
                    case PlayerMouse.AutoSelectMode.TileEntity:
                        return TileEntityAutoSelect;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(autoSelectMode), autoSelectMode, null);
                }
            }
        }
        [SerializeField] private Image image;
        [SerializeField] private AutoSelectSprites autoSelectSprites;
        private PlayerMouse.AutoSelectMode autoSelectMode;
        private PlayerMouse playerMouse;

        public void Initialize(PlayerMouse playerMouse)
        {
            this.playerMouse = playerMouse;
            Display();
        }
        public void Display()
        {
            image.sprite = autoSelectSprites.GetSprite(autoSelectMode);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Auto Select: {autoSelectMode}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int dir = eventData.button == PointerEventData.InputButton.Left ? 1 : -1;
            Iterate(dir);
        }

        public void Iterate(int dir)
        {
            autoSelectMode = GlobalHelper.ShiftEnum(dir, autoSelectMode);
            playerMouse.SetAutoSelect(autoSelectMode);
            Display();
        }

        public PlayerControl GetPlayerControl()
        {
            return PlayerControl.AutoSelect;
        }
    }
}
