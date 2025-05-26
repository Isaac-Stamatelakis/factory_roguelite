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
        private Dictionary<PlayerControl, ControlUIElement> elementUIDict = new Dictionary<PlayerControl, ControlUIElement>();
        
        public void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                CanvasController.Instance.PopStack();
                //ControlUtils.LoadBindings();
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
            PlayerScript playerScript = PlayerManager.Instance?.GetPlayer();
            InputActions inputActions = playerScript.InputActions;
            GlobalHelper.DeleteAllChildren(listTransform);
            PlayerControl[] playerControls = Enum.GetValues(typeof(PlayerControl)) as PlayerControl[];
            foreach (PlayerControl playerControl in playerControls)
            {
                ControlUIElement controlUIElement = Instantiate(controlUIElementPrefab, listTransform);
                controlUIElement.Initalize(playerControl,this,ControlUtils.GetPlayerControlBinding(playerControl));
            }
            /*
            Dictionary<string, ControlBindingCollection> sections = ControlUtils.GetKeyBindingSections();
            foreach (var kvp in sections)
            {
                TextMeshProUGUI header = Instantiate(headerPrefab, listTransform);
                header.text = kvp.Key;
                List<PlayerControl> bindings = kvp.Value.GetBindingKeys();
                foreach (PlayerControl binding in bindings)
                {
                    
                }
            }
            */
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
        public void OnDestroy()
        {
            PlayerScript playerScript = PlayerManager.Instance?.GetPlayer();
            if (!playerScript) return;
            playerScript.PlayerUIContainer.IndicatorManager.SyncKeyCodes(false);
        }
    }
}
