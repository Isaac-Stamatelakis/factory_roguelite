using System;
using PlayerModule;
using Robot.Tool;
using Robot.Tool.UI;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Player.Tool.UI
{
    public class PlayerToolListElementUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image mImage;
        private IRobotToolInstance robotToolInstance;
        private PlayerToolListUI parentUI;
        private Action<int> callback;
        private int index;
        public void Display(int index, IRobotToolInstance robotTool, Action<int> callback)
        {
            mImage.sprite = robotTool.GetSprite();
            this.robotToolInstance = robotTool;
            this.callback = callback;
            this.index = index;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, robotToolInstance?.GetName());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            callback?.Invoke(index);
        }
    }
}
