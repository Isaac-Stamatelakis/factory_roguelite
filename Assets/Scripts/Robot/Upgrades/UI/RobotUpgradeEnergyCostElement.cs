using System.Collections;
using System.Collections.Generic;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;

public class RobotUpgradeEnergyCostElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string text;

    public void SetDisplayText(string text)
    {
        this.text = text;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipController.Instance.ShowToolTip(transform.position,text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipController.Instance.HideToolTip();
    }
}
