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
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, GetDimensionText());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
        
    }
}
