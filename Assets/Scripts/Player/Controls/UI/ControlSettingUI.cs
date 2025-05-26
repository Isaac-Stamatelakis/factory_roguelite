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
            InputActions inputActions = CanvasController.Instance.InputActions;
            GlobalHelper.DeleteAllChildren(listTransform);
            Dictionary<PlayerControlGroup, List<PlayerControl>> sortedGroups = new Dictionary<PlayerControlGroup, List<PlayerControl>>();
            var playerControls = Enum.GetValues(typeof(PlayerControl));
            foreach (PlayerControl playerControl in playerControls)
            {
                PlayerControlGroup group = ControlUtils.GetGroup(playerControl);
                if (!sortedGroups.ContainsKey(group))
                {
                    sortedGroups.Add(group, new List<PlayerControl>());
                }
                sortedGroups[group].Add(playerControl);
            }

            foreach (var (group, groupControls) in sortedGroups)
            {
                TextMeshProUGUI header = GameObject.Instantiate(headerPrefab, listTransform);
                header.text = group.ToString();
                foreach (PlayerControl playerControl in groupControls)
                {
                    ControlUIElement controlUIElement = Instantiate(controlUIElementPrefab, listTransform);
                    controlUIElement.Initalize(playerControl,inputActions,this);
                    elementUIDict[playerControl] = controlUIElement;
                }
            }
            
            CheckConflicts();
        }
        
        public void CheckConflicts()
        {
            HashSet<PlayerControl> conflicts = ControlUtils.GetConflictingBindings();
            foreach (var (control, controlUIElement) in elementUIDict)
            {
                controlUIElement.HighlightConflictState(conflicts.Contains(control));
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
