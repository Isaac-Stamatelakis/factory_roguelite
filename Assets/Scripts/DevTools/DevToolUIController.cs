using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DevTools {
    public class DevToolUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject home;
        [SerializeField] private Button homeButton;
        private Transform currentUI;
        private string baseText;
        public void setTitleText(string text) {
            title.text = text;
        }
        public void setHomeVisibility(bool state) {
            home.SetActive(state);
            homeButton.gameObject.SetActive(!state);
        }
        public void addUI(Transform ui) {
            ui.transform.SetParent(transform,false);
            currentUI = ui;
        }
        public void resetTitleText() {
            title.text = baseText;
        }

        public void Start() {
            baseText = title.text;
            homeButton.onClick.AddListener(() => {
                setHomeVisibility(true);
                GameObject.Destroy(currentUI.gameObject);
                resetTitleText();
            });
            homeButton.gameObject.SetActive(false);
        }
    }

    public class DevToolUIControllerContainer {
        private static DevToolUIControllerContainer instance;
        private static DevToolUIController devToolUIController;
        private DevToolUIControllerContainer() {
            devToolUIController = GameObject.Find("UICanvas").GetComponent<DevToolUIController>();
        }
        public static DevToolUIController GetController() {
            if (instance == null) {
                instance = new DevToolUIControllerContainer();
            }
            return devToolUIController;
        }
        public static void reset() {
            instance = null;
        }
    }
}

