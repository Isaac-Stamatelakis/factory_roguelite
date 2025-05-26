using System;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Robot.Tool.UI
{
    public class PlayerToolUIIndicator : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ToolModeType toolModeType;
        [SerializeField] private Image image;
        private enum ToolModeType
        {
            Main,
            Secondary
        }

        private IRobotToolInstance displayedToolInstance;
        private int index;
        private PlayerToolListUI playerToolListUI;
        public void Display(IRobotToolInstance robotToolInstance, PlayerToolListUI playerToolListUI, int index)
        {
            displayedToolInstance = robotToolInstance;
            this.index = index;
            this.playerToolListUI = playerToolListUI;
            Refresh();
        }
        

        public void Refresh()
        {
            image.sprite = GetIndicatorSprite();
            playerToolListUI.RefreshToolSprite(index);
        }

        private Sprite GetIndicatorSprite()
        {
            switch (toolModeType)
            {
                case ToolModeType.Main:
                    return displayedToolInstance.GetPrimaryModeSprite();
                case ToolModeType.Secondary:
                    return displayedToolInstance is not ISubModeRobotToolInstance subModeRobotToolInstance ? null : subModeRobotToolInstance.GetSubModeSprite();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle) return;
            MoveDirection moveDirection = eventData.button == PointerEventData.InputButton.Left ? MoveDirection.Left : MoveDirection.Right;
            switch (toolModeType)
            {
                case ToolModeType.Main:
                    displayedToolInstance.ModeSwitch(moveDirection,false);
                    break;
                case ToolModeType.Secondary:
                    displayedToolInstance.ModeSwitch(moveDirection,true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Refresh();
            OnPointerEnter(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            string toolTipText = GetToolTipText();
            if (toolTipText == null) return;
            ToolTipController.Instance.ShowToolTip(transform.position,toolTipText);
        }

        private string GetToolTipText()
        {
            switch (toolModeType)
            {
                case ToolModeType.Main:
                    return displayedToolInstance.GetModeName();
                case ToolModeType.Secondary:
                    return displayedToolInstance is not ISubModeRobotToolInstance subModeRobotToolInstance ? null : subModeRobotToolInstance.GetSubModeName();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
