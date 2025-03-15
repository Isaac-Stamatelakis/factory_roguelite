using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Controls.UI
{
    public class ControlSettingUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Transform listTransform;
        [SerializeField] private TextMeshProUGUI headerPrefab;
        [SerializeField] private ControlUIElement controlUIElementPrefab;
        [SerializeField] private Button highlightConflicts;
        private Dictionary<PlayerControl, ControlUIElement> elementUIDict = new Dictionary<PlayerControl, ControlUIElement>();
        public bool ListeningToKey = false;
        public void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                CanvasController.Instance.PopStack();
                ControlUtils.LoadBindings();
            });
            restoreButton.onClick.AddListener(() =>
            {
                ControlUtils.SetDefault();
                Display();
            });
            Display();
        }
        
        private void Display()
        {
            Dictionary<string, ControlBindingCollection> sections = ControlUtils.GetKeyBindingSections();
            foreach (var kvp in sections)
            {
                TextMeshProUGUI header = Instantiate(headerPrefab, listTransform);
                header.text = kvp.Key;
                List<PlayerControl> bindings = kvp.Value.GetBindingKeys();
                foreach (PlayerControl binding in bindings)
                {
                    ControlUIElement controlUIElement = Instantiate(controlUIElementPrefab, listTransform);
                    controlUIElement.Initalize(binding,this);
                    elementUIDict[binding] = controlUIElement;
                }
            }
            CheckConflicts();
        }
        
        public void CheckConflicts()
        {
            HashSet<PlayerControl> conflicts = ControlUtils.GetConflictingBindings();
            foreach (var kvp in elementUIDict)
            {
                ControlUIElement controlUIElement = kvp.Value;
                controlUIElement.HighlightConflictState(conflicts.Contains(kvp.Key));
            }
        }

        public void Update()
        {
            if (ListeningToKey) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CanvasController.Instance.PopStack();
            }
        }
    }
}
