using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerModule {
    public class PlayerRobotToolUI : MonoBehaviour
    {
        
        private static string panelPath = "UI/Player/ToolPanel";
        [SerializeField] private GridLayoutGroup toolList;
        public void Start() {
            for (int i = 0; i < 10; i++) {
                GameObject panel = GlobalHelper.instantiateFromResourcePath(panelPath);
                panel.transform.SetParent(toolList.transform,false);
            }
        }
    }
}

