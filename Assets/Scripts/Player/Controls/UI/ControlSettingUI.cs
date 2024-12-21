using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        private Dictionary<string, ControlUIElement> elementUIDict = new Dictionary<string, ControlUIElement>();
        public void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });
            restoreButton.onClick.AddListener(ControlUtils.SetDefault);
            Display();
        }
        
        private void Display()
        {
            Dictionary<string, ControlBindingCollection> sections = ControlUtils.GetKeyBindingSections();
            foreach (var kvp in sections)
            {
                TextMeshProUGUI header = Instantiate(headerPrefab, listTransform);
                header.text = kvp.Key;
                List<string> bindings = kvp.Value.GetBindingKeys();
                foreach (string binding in bindings)
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
            HashSet<string> conflicts = ControlUtils.GetConflictingBindings();
            foreach (var kvp in elementUIDict)
            {
                ControlUIElement controlUIElement = kvp.Value;
                controlUIElement.HighlightConflictState(conflicts.Contains(kvp.Key));
            }
        }
    }
}
