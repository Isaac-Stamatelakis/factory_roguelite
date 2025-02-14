using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileRotationIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image tileImage;
        private int currentRotation;
        public void Display(int rotation)
        {
            currentRotation = rotation;
            tileImage.transform.rotation = Quaternion.Euler(0, 90*rotation, 0);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Tile Rotation:  {90*currentRotation}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
    }
}
