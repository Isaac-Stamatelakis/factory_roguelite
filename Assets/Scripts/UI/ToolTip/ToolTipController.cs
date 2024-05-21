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

        public void showToolTip(Vector2 position, string text) {
            hideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(toolTipPrefab);
            newToolTip.setText(text);
            newToolTip.transform.SetParent(transform,false);
            newToolTip.transform.position = position;
        }
        public void hideToolTip() {
            GlobalHelper.deleteAllChildren(transform);
        }
    }
}

