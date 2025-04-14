using System;
using Player;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class CaveIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image mImage;
        private Vector2 returnPortalLocation;
        private PlayerScript playerScript;
        private float distance;
        private bool hovering = false;
        
        public void SyncToSystem(PlayerScript playerScript, Vector2 portalLocation)
        {
            this.playerScript = playerScript;
            returnPortalLocation = portalLocation;
        }

        public void Update()
        {
            if (!playerScript) return;
            Vector2 dif = returnPortalLocation-(Vector2)playerScript.transform.position;
            Vector2 direction = dif.normalized;
            distance = dif.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            mImage.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private string GetDimensionText()
        {
            return $"Distance From Portal:{distance:F1}m";
        }

        public void FixedUpdate()
        {
            if (hovering) Display();
        }

        public void Display()
        {
            ToolTipController.Instance.ShowToolTip(transform.position, GetDimensionText());
        }
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            Display();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            ToolTipController.Instance.HideToolTip();
        }
        
    }
}
