using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.ToolTip {
    public class ToolTipController : MonoBehaviour
    {
        [SerializeField] private ToolTipUI toolTipPrefab;
        private static ToolTipController instance;

        public static ToolTipController Instance { get => instance; }

        public void Awake() {
            instance = this;
        }

        public void ShowToolTip(Vector2 position, string text) {
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(toolTipPrefab, transform, false);
            newToolTip.setText(text);
            newToolTip.transform.position = position;
        }
        public void HideToolTip() {
            GlobalHelper.deleteAllChildren(transform);
        }
    }
}

